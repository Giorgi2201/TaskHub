import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../user.service';
import { AuthService } from '../auth.service';
import { DigestDraftService } from '../digest-draft.service';
import { ToastService } from '../toast.service';
import { Subscription } from 'rxjs';

interface User {
  userID: number;
  name: string;
  role: string;
  department: string;
  email: string;
  title: string;
}

interface NewsItem {
  newsID: number;
  title: string;
  content: string;
  imageUrl?: string;
  authorName: string;
  createdAt: string;
}

interface VacancyItem {
  vacancyID: number;
  title: string;
  category: string;
  department: string;
  location: string;
  description: string;
  deadline: string;
  isActive: boolean;
  authorName: string;
  createdAt: string;
}

interface DigestItem {
  digestEntryID: number;
  title: string;
  description: string;
  imageUrl?: string;
  sourceName: string;
  sourceUrl: string;
  periodFrom: string;
  periodTo: string;
  isFeatured: boolean;
  isActive: boolean;
  authorName: string;
  createdAt: string;
}

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit, OnDestroy {
  activeTab: 'users' | 'news' | 'vacancies' | 'digest' = 'users';
  loading = false;
  private loadCounter = 0;
  users: User[] = [];
  news: NewsItem[] = [];
  vacancies: VacancyItem[] = [];
  digestEntries: DigestItem[] = [];
  modalOpen = false;
  isEditMode = false;
  isAdmin = false;
  private userSubscription?: Subscription;

  formData: User = {
    userID: 0,
    name: '',
    role: '',
    department: '',
    email: '',
    title: ''
  };

  newsFormData: NewsItem = {
    newsID: 0,
    title: '',
    content: '',
    imageUrl: '',
    authorName: '',
    createdAt: ''
  };

  vacancyFormData: VacancyItem = {
    vacancyID: 0,
    title: '',
    category: '',
    department: '',
    location: '',
    description: '',
    deadline: '',
    isActive: true,
    authorName: '',
    createdAt: ''
  };

  vacancyCategories = ['ღია საჯარო კონკურსი', 'შიდა კონკურსი', 'სააპლიკაციო ფორმა'];
  roles = ['შემვსები', 'ხელმძღვანელი', 'შემსრულებელი', 'ადმინისტრატორი'];

  // Subscription to "digest entry saved" events from the shared draft service, so
  // this list stays in sync even though the save action is now triggered from the
  // globally-rendered digest modal (owned by DigestDraftService) rather than here.
  private digestSavedSubscription?: Subscription;

  get minDeadlineDate(): string {
    const now = new Date();
    now.setSeconds(0, 0);
    return now.toISOString().slice(0, 16);
  }

  constructor(
    private userService: UserService,
    private router: Router,
    private authService: AuthService,
    // Public: the template calls draftService.openAddDigestModal()/openEditDigestModal()
    // directly, since the digest modal itself is now owned and rendered by AppComponent.
    public draftService: DigestDraftService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.userSubscription = this.userService.currentUser$.subscribe(user => {
      this.isAdmin = user.role === 'ადმინისტრატორი';
      if (!this.isAdmin) {
        this.router.navigate(['/requests']);
      }
    });

    this.loadUsers();
    this.loadNews();
    this.loadVacancies();
    this.loadDigestEntries();

    // Refresh the entries list whenever a digest entry is created/updated via the
    // globally-rendered modal (the save action no longer necessarily happens while
    // this component is even mounted, so we can't rely on a local callback).
    this.digestSavedSubscription = this.draftService.digestSaved$.subscribe(() => {
      this.loadDigestEntries();
    });
  }

  ngOnDestroy(): void {
    this.userSubscription?.unsubscribe();
    this.digestSavedSubscription?.unsubscribe();
  }

  private decrementLoad(): void {
    this.loadCounter--;
    if (this.loadCounter <= 0) { this.loadCounter = 0; this.loading = false; }
  }

  loadUsers(): void {
    this.loadCounter++; this.loading = true;
    this.userService.getUsers().subscribe({
      next: (users) => { this.users = users; this.decrementLoad(); },
      error: (error) => { console.error('Error loading users:', error); this.decrementLoad(); this.toastService.showError('მომხმარებლების ჩატვირთვა ვერ მოხერხდა'); }
    });
  }

  loadNews(): void {
    this.loadCounter++; this.loading = true;
    this.userService.getNews().subscribe({
      next: (news) => { this.news = news; this.decrementLoad(); },
      error: (error) => { console.error('Error loading news:', error); this.decrementLoad(); this.toastService.showError('სიახლეების ჩატვირთვა ვერ მოხერხდა'); }
    });
  }

  loadVacancies(): void {
    this.loadCounter++; this.loading = true;
    this.userService.getVacancies().subscribe({
      next: (vacancies) => { this.vacancies = vacancies; this.decrementLoad(); },
      error: (error) => { console.error('Error loading vacancies:', error); this.decrementLoad(); this.toastService.showError('ვაკანსიების ჩატვირთვა ვერ მოხერხდა'); }
    });
  }

  loadDigestEntries(): void {
    this.loadCounter++; this.loading = true;
    this.userService.getDigestEntries().subscribe({
      next: (entries) => { this.digestEntries = entries; this.decrementLoad(); },
      error: (error) => { console.error('Error loading digest entries:', error); this.decrementLoad(); this.toastService.showError('ჩანაწერების ჩატვირთვა ვერ მოხერხდა'); }
    });
  }

  getRoleClass(role: string): string {
    const roleMap: { [key: string]: string } = {
      'შემვსები': 'role-submitter',
      'ხელმძღვანელი': 'role-supervisor',
      'შემსრულებელი': 'role-executor',
      'ადმინისტრატორი': 'role-admin'
    };
    return roleMap[role] || 'role-submitter';
  }

  getCategoryClass(category: string): string {
    const categoryMap: { [key: string]: string } = {
      'ღია საჯარო კონკურსი': 'category-open',
      'შიდა კონკურსი': 'category-internal',
      'სააპლიკაციო ფორმა': 'category-application'
    };
    return categoryMap[category] || 'category-open';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return dateStr;
    const months = ['იან', 'თებ', 'მარ', 'აპრ', 'მაი', 'ივნ', 'ივლ', 'აგვ', 'სექ', 'ოქტ', 'ნოე', 'დეკ'];
    return `${date.getDate()} ${months[date.getMonth()]} ${date.getFullYear()}`;
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.formData = { userID: 0, name: '', role: '', department: '', email: '', title: '' };
    this.modalOpen = true;
  }

  openEditModal(user: User): void {
    this.isEditMode = true;
    this.formData = { ...user };
    this.modalOpen = true;
  }

  closeModal(): void {
    this.modalOpen = false;
    this.formData = { userID: 0, name: '', role: '', department: '', email: '', title: '' };
    this.newsFormData = { newsID: 0, title: '', content: '', imageUrl: '', authorName: '', createdAt: '' };
    this.vacancyFormData = {
      vacancyID: 0, title: '', category: '', department: '',
      location: '', description: '', deadline: '', isActive: true,
      authorName: '', createdAt: ''
    };
  }

  // Note: Escape-to-close for the digest modal is handled by AppComponent, since
  // that modal is no longer part of this component's `modalOpen` state.
  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: any): void {
    if (this.modalOpen) {
      this.closeModal();
    }
  }

  saveUser(): void {
    if (!this.formData.name || !this.formData.role || !this.formData.department ||
        !this.formData.email || !this.formData.title) {
      this.toastService.showWarning('გთხოვთ შეავსოთ ყველა ველი');
      return;
    }

    if (this.isEditMode) {
      this.userService.updateUser(this.formData.userID, this.formData).subscribe({
        next: () => { this.loadUsers(); this.closeModal(); this.toastService.showSuccess('მომხმარებელი განახლდა'); },
        error: () => { this.toastService.showError('შეცდომა მომხმარებლის განახლებისას'); }
      });
    } else {
      this.userService.createUser(this.formData).subscribe({
        next: () => { this.loadUsers(); this.closeModal(); this.toastService.showSuccess('მომხმარებელი დაემატა'); },
        error: () => { this.toastService.showError('შეცდომა მომხმარებლის შექმნისას'); }
      });
    }
  }

  deleteUser(userId: number): void {
    if (confirm('ნამდვილად გსურთ მომხმარებლის წაშლა?')) {
      this.userService.deleteUser(userId).subscribe({
        next: () => { this.loadUsers(); this.toastService.showSuccess('მომხმარებელი წაიშალა'); },
        error: () => { this.toastService.showError('შეცდომა მომხმარებლის წაშლისას'); }
      });
    }
  }

  openAddNewsModal(): void {
    this.isEditMode = false;
    this.newsFormData = { newsID: 0, title: '', content: '', imageUrl: '', authorName: '', createdAt: '' };
    this.modalOpen = true;
  }

  openEditNewsModal(newsItem: NewsItem): void {
    this.isEditMode = true;
    this.newsFormData = { ...newsItem };
    this.modalOpen = true;
  }

  saveNews(): void {
    if (!this.newsFormData.title || !this.newsFormData.content) {
      this.toastService.showWarning('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode) {
      const updateData = {
        title: this.newsFormData.title,
        content: this.newsFormData.content,
        imageUrl: this.newsFormData.imageUrl || null
      };
      this.userService.updateNews(this.newsFormData.newsID, updateData).subscribe({
        next: () => { this.loadNews(); this.closeModal(); this.toastService.showSuccess('სიახლე განახლდა'); },
        error: () => { this.toastService.showError('შეცდომა სიახლის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { this.toastService.showError('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = {
        title: this.newsFormData.title,
        content: this.newsFormData.content,
        imageUrl: this.newsFormData.imageUrl || null,
        authorID: currentUser.userId
      };
      this.userService.createNews(createData).subscribe({
        next: () => { this.loadNews(); this.closeModal(); this.toastService.showSuccess('სიახლე დაემატა'); },
        error: () => { this.toastService.showError('შეცდომა სიახლის შექმნისას'); }
      });
    }
  }

  deleteNews(newsId: number): void {
    if (confirm('ნამდვილად გსურთ სიახლის წაშლა?')) {
      this.userService.deleteNews(newsId).subscribe({
        next: () => { this.loadNews(); this.toastService.showSuccess('სიახლე წაიშალა'); },
        error: () => { this.toastService.showError('შეცდომა სიახლის წაშლისას'); }
      });
    }
  }

  openAddVacancyModal(): void {
    this.isEditMode = false;
    this.vacancyFormData = {
      vacancyID: 0, title: '', category: '', department: '',
      location: '', description: '', deadline: '', isActive: true,
      authorName: '', createdAt: ''
    };
    this.modalOpen = true;
  }

  openEditVacancyModal(vacancy: VacancyItem): void {
    this.isEditMode = true;
    this.vacancyFormData = { ...vacancy };
    this.modalOpen = true;
  }

  saveVacancy(): void {
    if (!this.vacancyFormData.title || !this.vacancyFormData.category || !this.vacancyFormData.description) {
      this.toastService.showWarning('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode) {
      this.userService.updateVacancy(this.vacancyFormData.vacancyID, this.vacancyFormData).subscribe({
        next: () => { this.loadVacancies(); this.closeModal(); this.toastService.showSuccess('ვაკანსია განახლდა'); },
        error: () => { this.toastService.showError('შეცდომა ვაკანსიის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { this.toastService.showError('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = { ...this.vacancyFormData, authorID: currentUser.userId };
      this.userService.createVacancy(createData).subscribe({
        next: () => { this.loadVacancies(); this.closeModal(); this.toastService.showSuccess('ვაკანსია დაემატა'); },
        error: () => { this.toastService.showError('შეცდომა ვაკანსიის შექმნისას'); }
      });
    }
  }

  deleteVacancy(vacancyId: number): void {
    if (confirm('ნამდვილად გსურთ ვაკანსიის წაშლა?')) {
      this.userService.deleteVacancy(vacancyId).subscribe({
        next: () => { this.loadVacancies(); this.toastService.showSuccess('ვაკანსია წაიშალა'); },
        error: () => { this.toastService.showError('შეცდომა ვაკანსიის წაშლისას'); }
      });
    }
  }

  // Adding/editing a digest entry now opens the app-wide modal owned by
  // DigestDraftService (rendered in AppComponent, outside <router-outlet>), so it
  // stays interactive across every route rather than being tied to this page.
  // The (single-minimized-draft) confirmation guard lives in the service too.

  deleteDigestEntry(entryId: number): void {
    if (confirm('ნამდვილად გსურთ ჩანაწერის წაშლა?')) {
      this.userService.deleteDigestEntry(entryId).subscribe({
        next: () => { this.loadDigestEntries(); this.toastService.showSuccess('ჩანაწერი წაიშალა'); },
        error: () => { this.toastService.showError('შეცდომა ჩანაწერის წაშლისას'); }
      });
    }
  }
}
