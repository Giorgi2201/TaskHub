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

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit, OnDestroy {
  activeTab: 'users' | 'news' = 'users';
  users: User[] = [];
  news: NewsItem[] = [];
  modalOpen = false;
  isEditMode = false;
  isAdmin = false;
  private userSubscription?: Subscription;
  
  // User form data
  formData: User = {
    userID: 0,
    name: '',
    role: '',
    department: '',
    email: '',
    title: ''
  };

  // News form data
  newsFormData: NewsItem = {
    newsID: 0,
    title: '',
    content: '',
    imageUrl: '',
    authorName: '',
    createdAt: ''
  };

  roles = ['შემვსები', 'ხელმძღვანელი', 'შემსრულებელი', 'ადმინისტრატორი'];

  constructor(
    private userService: UserService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Check if user is admin
    this.userSubscription = this.userService.currentUser$.subscribe(user => {
      this.isAdmin = user.role === 'ადმინისტრატორი';
      if (!this.isAdmin) {
        // Redirect to requests page if not admin
        this.router.navigate(['/requests']);
      }
    });

    this.loadUsers();
    this.loadNews();
  }

  ngOnDestroy(): void {
    this.userSubscription?.unsubscribe();
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe({
      next: (users) => {
        this.users = users;
      },
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
  }

  loadNews(): void {
    this.userService.getNews().subscribe({
      next: (news) => {
        this.news = news;
      },
      error: (error) => {
        console.error('Error loading news:', error);
      }
    });
  }

  getRoleClass(role: string): string {
    const roleMap: { [key: string]: string } = {
      'შემვსები': 'role-submitter',
      'ხელმძღვანელი': 'role-supervisor', // Green
      'შემსრულებელი': 'role-executor', // Orange
      'ადმინისტრატორი': 'role-admin'
    };
    return roleMap[role] || 'role-submitter';
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.formData = {
      userID: 0,
      name: '',
      role: '',
      department: '',
      email: '',
      title: ''
    };
    this.modalOpen = true;
  }

  openEditModal(user: User): void {
    this.isEditMode = true;
    this.formData = { ...user };
    this.modalOpen = true;
  }

  closeModal(): void {
    this.modalOpen = false;
    this.formData = {
      userID: 0,
      name: '',
      role: '',
      department: '',
      email: '',
      title: ''
    };
    this.newsFormData = {
      newsID: 0,
      title: '',
      content: '',
      imageUrl: '',
      authorName: '',
      createdAt: ''
    };
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
        next: () => {
          this.loadUsers();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error updating user:', error);
          alert('შეცდომა მომხმარებლის განახლებისას');
        }
      });
    } else {
      this.userService.createUser(this.formData).subscribe({
        next: () => {
          this.loadUsers();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error creating user:', error);
          alert('შეცდომა მომხმარებლის შექმნისას');
        }
      });
    }
  }

  deleteUser(userId: number): void {
    if (confirm('ნამდვილად გსურთ მომხმარებლის წაშლა?')) {
      this.userService.deleteUser(userId).subscribe({
        next: () => {
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          alert('შეცდომა მომხმარებლის წაშლისას');
        }
      });
    }
  }

  // News methods
  openAddNewsModal(): void {
    this.isEditMode = false;
    this.newsFormData = {
      newsID: 0,
      title: '',
      content: '',
      imageUrl: '',
      authorName: '',
      createdAt: ''
    };
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
        next: () => {
          this.loadNews();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error updating news:', error);
          alert('შეცდომა სიახლის განახლებისას');
        }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) {
        alert('მომხმარებელი არ არის ავტორიზებული');
        return;
      }

      const createData = {
        title: this.newsFormData.title,
        content: this.newsFormData.content,
        imageUrl: this.newsFormData.imageUrl || null,
        authorID: currentUser.userId
      };

      this.userService.createNews(createData).subscribe({
        next: () => {
          this.loadNews();
          this.closeModal();
        },
        error: (error) => {
          console.error('Error creating news:', error);
          alert('შეცდომა სიახლის შექმნისას');
        }
      });
    }
  }

  deleteNews(newsId: number): void {
    if (confirm('ნამდვილად გსურთ სიახლის წაშლა?')) {
      this.userService.deleteNews(newsId).subscribe({
        next: () => {
          this.loadNews();
        },
        error: (error) => {
          console.error('Error deleting news:', error);
          alert('შეცდომა სიახლის წაშლისას');
        }
      });
    }
  }
}
