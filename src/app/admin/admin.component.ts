import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../user.service';
import { AuthService } from '../auth.service';
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

  digestFormData: DigestItem = {
    digestEntryID: 0,
    title: '',
    description: '',
    imageUrl: '',
    sourceName: '',
    sourceUrl: '',
    periodFrom: '',
    periodTo: '',
    isFeatured: false,
    isActive: true,
    authorName: '',
    createdAt: ''
  };

  digestPhotoMode: 'url' | 'upload' = 'url';
  digestUploadedFile: File | null = null;

  get minDeadlineDate(): string {
    const now = new Date();
    now.setSeconds(0, 0);
    return now.toISOString().slice(0, 16);
  }

  constructor(
    private userService: UserService,
    private router: Router,
    private authService: AuthService
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
  }

  ngOnDestroy(): void {
    this.userSubscription?.unsubscribe();
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe({
      next: (users) => { this.users = users; },
      error: (error) => { console.error('Error loading users:', error); }
    });
  }

  loadNews(): void {
    this.userService.getNews().subscribe({
      next: (news) => { this.news = news; },
      error: (error) => { console.error('Error loading news:', error); }
    });
  }

  loadVacancies(): void {
    this.userService.getVacancies().subscribe({
      next: (vacancies) => { this.vacancies = vacancies; },
      error: (error) => { console.error('Error loading vacancies:', error); }
    });
  }

  loadDigestEntries(): void {
    this.userService.getDigestEntries().subscribe({
      next: (entries) => { this.digestEntries = entries; },
      error: (error) => { console.error('Error loading digest entries:', error); }
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
    this.digestFormData = {
      digestEntryID: 0, title: '', description: '', imageUrl: '',
      sourceName: '', sourceUrl: '', periodFrom: '', periodTo: '',
      isFeatured: false, isActive: true, authorName: '', createdAt: ''
    };
    this.digestPhotoMode = 'url';
    this.digestUploadedFile = null;
  }

  @HostListener('document:keydown.escape', ['$event'])
  handleEscape(event: any): void {
    if (this.modalOpen) {
      this.closeModal();
    }
  }

  saveUser(): void {
    if (!this.formData.name || !this.formData.role || !this.formData.department ||
        !this.formData.email || !this.formData.title) {
      alert('გთხოვთ შეავსოთ ყველა ველი');
      return;
    }

    if (this.isEditMode) {
      this.userService.updateUser(this.formData.userID, this.formData).subscribe({
        next: () => { this.loadUsers(); this.closeModal(); },
        error: (error) => { console.error('Error updating user:', error); alert('შეცდომა მომხმარებლის განახლებისას'); }
      });
    } else {
      this.userService.createUser(this.formData).subscribe({
        next: () => { this.loadUsers(); this.closeModal(); },
        error: (error) => { console.error('Error creating user:', error); alert('შეცდომა მომხმარებლის შექმნისას'); }
      });
    }
  }

  deleteUser(userId: number): void {
    if (confirm('ნამდვილად გსურთ მომხმარებლის წაშლა?')) {
      this.userService.deleteUser(userId).subscribe({
        next: () => { this.loadUsers(); },
        error: (error) => { console.error('Error deleting user:', error); alert('შეცდომა მომხმარებლის წაშლისას'); }
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
      alert('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode) {
      const updateData = {
        title: this.newsFormData.title,
        content: this.newsFormData.content,
        imageUrl: this.newsFormData.imageUrl || null
      };
      this.userService.updateNews(this.newsFormData.newsID, updateData).subscribe({
        next: () => { this.loadNews(); this.closeModal(); },
        error: (error) => { console.error('Error updating news:', error); alert('შეცდომა სიახლის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { alert('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = {
        title: this.newsFormData.title,
        content: this.newsFormData.content,
        imageUrl: this.newsFormData.imageUrl || null,
        authorID: currentUser.userId
      };
      this.userService.createNews(createData).subscribe({
        next: () => { this.loadNews(); this.closeModal(); },
        error: (error) => { console.error('Error creating news:', error); alert('შეცდომა სიახლის შექმნისას'); }
      });
    }
  }

  deleteNews(newsId: number): void {
    if (confirm('ნამდვილად გსურთ სიახლის წაშლა?')) {
      this.userService.deleteNews(newsId).subscribe({
        next: () => { this.loadNews(); },
        error: (error) => { console.error('Error deleting news:', error); alert('შეცდომა სიახლის წაშლისას'); }
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
      alert('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode) {
      this.userService.updateVacancy(this.vacancyFormData.vacancyID, this.vacancyFormData).subscribe({
        next: () => { this.loadVacancies(); this.closeModal(); },
        error: (error) => { console.error('Error updating vacancy:', error); alert('შეცდომა ვაკანსიის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { alert('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = { ...this.vacancyFormData, authorID: currentUser.userId };
      this.userService.createVacancy(createData).subscribe({
        next: () => { this.loadVacancies(); this.closeModal(); },
        error: (error) => { console.error('Error creating vacancy:', error); alert('შეცდომა ვაკანსიის შექმნისას'); }
      });
    }
  }

  deleteVacancy(vacancyId: number): void {
    if (confirm('ნამდვილად გსურთ ვაკანსიის წაშლა?')) {
      this.userService.deleteVacancy(vacancyId).subscribe({
        next: () => { this.loadVacancies(); },
        error: (error) => { console.error('Error deleting vacancy:', error); alert('შეცდომა ვაკანსიის წაშლისას'); }
      });
    }
  }

  openAddDigestModal(): void {
    this.isEditMode = false;
    this.digestFormData = {
      digestEntryID: 0, title: '', description: '', imageUrl: '',
      sourceName: '', sourceUrl: '', periodFrom: '', periodTo: '',
      isFeatured: false, isActive: true, authorName: '', createdAt: ''
    };
    this.digestPhotoMode = 'url';
    this.digestUploadedFile = null;
    this.modalOpen = true;
  }

  openEditDigestModal(entry: DigestItem): void {
    this.isEditMode = true;
    this.digestFormData = { ...entry };
    this.digestPhotoMode = 'url';
    this.digestUploadedFile = null;
    this.modalOpen = true;
  }

  onDigestFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.digestUploadedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = (e) => {
        this.digestFormData.imageUrl = e.target?.result as string;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  saveDigestEntry(): void {
    if (!this.digestFormData.title || !this.digestFormData.description ||
        !this.digestFormData.periodFrom || !this.digestFormData.periodTo) {
      alert('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode) {
      const updateData = {
        title: this.digestFormData.title,
        description: this.digestFormData.description,
        imageUrl: this.digestFormData.imageUrl || null,
        sourceName: this.digestFormData.sourceName,
        sourceUrl: this.digestFormData.sourceUrl,
        periodFrom: this.digestFormData.periodFrom,
        periodTo: this.digestFormData.periodTo,
        isFeatured: this.digestFormData.isFeatured,
        isActive: this.digestFormData.isActive
      };
      this.userService.updateDigestEntry(this.digestFormData.digestEntryID, updateData).subscribe({
        next: () => { this.loadDigestEntries(); this.closeModal(); },
        error: (error) => { console.error('Error updating digest entry:', error); alert('შეცდომა ჩანაწერის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { alert('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = {
        title: this.digestFormData.title,
        description: this.digestFormData.description,
        imageUrl: this.digestFormData.imageUrl || null,
        sourceName: this.digestFormData.sourceName,
        sourceUrl: this.digestFormData.sourceUrl,
        periodFrom: this.digestFormData.periodFrom,
        periodTo: this.digestFormData.periodTo,
        isFeatured: this.digestFormData.isFeatured,
        isActive: this.digestFormData.isActive,
        authorID: currentUser.userId
      };
      this.userService.createDigestEntry(createData).subscribe({
        next: () => { this.loadDigestEntries(); this.closeModal(); },
        error: (error) => { console.error('Error creating digest entry:', error); alert('შეცდომა ჩანაწერის შექმნისას'); }
      });
    }
  }

  deleteDigestEntry(entryId: number): void {
    if (confirm('ნამდვილად გსურთ ჩანაწერის წაშლა?')) {
      this.userService.deleteDigestEntry(entryId).subscribe({
        next: () => { this.loadDigestEntries(); },
        error: (error) => { console.error('Error deleting digest entry:', error); alert('შეცდომა ჩანაწერის წაშლისას'); }
      });
    }
  }
}
