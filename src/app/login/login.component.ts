import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService, DemoUser } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  showDemoPanel = false;
  demoUsers: DemoUser[] = [];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  login(): void {
    this.errorMessage = '';

    if (!this.email || !this.password) {
      this.errorMessage = 'გთხოვთ შეავსოთ ყველა ველი';
      return;
    }

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (user) => {
        // Sync with UserService for role-based UI
        this.router.navigate(['/requests']);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'შესვლა ვერ მოხერხდა';
      }
    });
  }

  toggleDemoPanel(): void {
    this.showDemoPanel = !this.showDemoPanel;
    
    if (this.showDemoPanel && this.demoUsers.length === 0) {
      this.loadDemoCredentials();
    }
  }

  loadDemoCredentials(): void {
    this.authService.getDemoCredentials().subscribe({
      next: (users) => {
        this.demoUsers = users;
      },
      error: (error) => {
        console.error('Failed to load demo credentials:', error);
      }
    });
  }

  useDemoCredentials(user: DemoUser): void {
    this.email = user.email;
    this.password = user.password;
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
}
