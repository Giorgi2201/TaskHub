import { Component, HostListener, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { filter } from 'rxjs';
import { UserService, CurrentUser } from './user.service';
import { AuthService } from './auth.service';
import { DigestDraftService } from './digest-draft.service';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'taskhub';
  userDropdownOpen = false;
  currentUserRole = 'შემვსები';
  isAuthenticated = false;
  showHeader = true;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    public draftService: DigestDraftService,
    public toastService: ToastService
  ) {
    // Hide header on login page
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.showHeader = !event.url.includes('/login');
    });

    // Subscribe to auth state
    this.authService.currentUser$.subscribe(user => {
      this.isAuthenticated = user !== null;
      if (user) {
        // Sync with UserService
        this.userService.setUserByRole(user.role);
        this.currentUserRole = user.role;
        // Load any minimized digest draft so the global bubble reappears app-wide
        // (including after a page refresh). Only admins can own a digest draft.
        if (user.role === 'ადმინისტრატორი') {
          this.draftService.init(user.userId);
        }
      } else {
        // On logout, hide the bubble/modal (server-side draft is kept for next login).
        this.draftService.reset();
      }
    });

    // Subscribe to current user to track role changes
    this.userService.currentUser$.subscribe(user => {
      this.currentUserRole = user.role;
    });
  }

  ngOnInit(): void {
    // Initialize
  }

  toggleUserDropdown() {
    this.userDropdownOpen = !this.userDropdownOpen;
  }

  getCurrentUser() {
    const authUser = this.authService.getCurrentUser();
    if (authUser) {
      return {
        name: authUser.name,
        initials: authUser.initials,
        title: authUser.title,
        role: authUser.role
      };
    }
    return {
      name: 'მომხმარებელი',
      initials: 'მ',
      title: 'თანამდებობა',
      role: 'შემვსები'
    };
  }

  getRoleClass(): string {
    const role = this.getCurrentUser().role;
    const roleMap: { [key: string]: string } = {
      'შემვსები': 'role-submitter',
      'ხელმძღვანელი': 'role-supervisor',
      'შემსრულებელი': 'role-executor',
      'ადმინისტრატორი': 'role-admin'
    };
    return roleMap[role] || 'role-submitter';
  }

  isAdmin(): boolean {
    return this.currentUserRole === 'ადმინისტრატორი';
  }

  logout(): void {
    this.authService.logout();
    this.userDropdownOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const userDropdown = target.closest('.user-dropdown-wrapper');
    
    if (!userDropdown && this.userDropdownOpen) {
      this.userDropdownOpen = false;
    }
  }

  // Escape closes the globally-rendered digest modal (or, if the themed
  // discard-draft confirmation is up, cancels that first). This is intentionally
  // separate from AdminComponent's own Escape handler (which only covers the
  // users/news/vacancies modals), since the digest modal no longer lives there.
  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.draftService.isConfirmDialogVisible) {
      this.draftService.resolveConfirmDialog(false);
    } else if (this.draftService.isModalOpen) {
      this.draftService.closeAndDiscard();
    }
  }
}
