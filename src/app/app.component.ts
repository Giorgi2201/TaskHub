import { Component, HostListener, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { filter } from 'rxjs';
import { UserService, CurrentUser } from './user.service';
import { AuthService } from './auth.service';
import { DigestDraftService } from './digest-draft.service';
import { ToastService } from './toast.service';
import { ProfileService, ProfileInfo } from './profile.service';

// Field-level errors for the change-password form. Keyed by field name so the
// template can show each message right under its input, mirroring the
// `field` property the backend returns on 400 responses.
interface PasswordFormErrors {
  oldPassword?: string;
  newPassword?: string;
  confirmNewPassword?: string;
}

const MIN_PASSWORD_LENGTH = 8; // mirrors ProfileController's MinimumPasswordLength

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'taskhub';
  userDropdownOpen = false;
  profileModalOpen = false;
  currentUserRole = 'შემვსები';
  isAuthenticated = false;
  showHeader = true;

  // ── Profile modal state ─────────────────────────────────────────────
  profile: ProfileInfo | null = null;
  profileLoading = false;

  passwordForm = { oldPassword: '', newPassword: '', confirmNewPassword: '' };
  passwordErrors: PasswordFormErrors = {};
  passwordSaving = false;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private profileService: ProfileService,
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

  openProfileModal(): void {
    this.profileModalOpen = true;
    this.userDropdownOpen = false;
    this.resetPasswordForm();

    const userId = this.authService.getCurrentUser()?.userId;
    if (userId == null) return;

    this.profile = null;
    this.profileLoading = true;
    this.profileService.getProfile(userId).subscribe({
      next: (profile) => {
        this.profile = profile;
        this.profileLoading = false;
      },
      error: () => {
        this.profileLoading = false;
        this.toastService.showError('პროფილის ჩატვირთვა ვერ მოხერხდა');
      }
    });
  }

  closeProfileModal(): void {
    // Warn before losing typed password input, same UX pattern used for the
    // digest draft's discard-confirmation (a plain confirm() is fine here —
    // this form has no "minimize" concept worth a themed dialog).
    if (this.hasUnsavedPasswordInput() && !confirm('პაროლის ველებში შეყვანილი მონაცემები დაიკარგება. დახურვა?')) {
      return;
    }
    this.profileModalOpen = false;
    this.resetPasswordForm();
  }

  private hasUnsavedPasswordInput(): boolean {
    const f = this.passwordForm;
    return !!(f.oldPassword || f.newPassword || f.confirmNewPassword);
  }

  private resetPasswordForm(): void {
    this.passwordForm = { oldPassword: '', newPassword: '', confirmNewPassword: '' };
    this.passwordErrors = {};
  }

  // Client-side validation mirroring ProfileController.ChangePassword's rules,
  // for instant feedback before ever hitting the backend. Returns true if the
  // form is valid; otherwise populates passwordErrors and returns false.
  private validatePasswordForm(): boolean {
    const errors: PasswordFormErrors = {};
    const { oldPassword, newPassword, confirmNewPassword } = this.passwordForm;

    if (!oldPassword) {
      errors.oldPassword = 'შეიყვანეთ მიმდინარე პაროლი';
    }

    if (!newPassword || newPassword.length < MIN_PASSWORD_LENGTH) {
      errors.newPassword = `ახალი პაროლი უნდა შედგებოდეს მინიმუმ ${MIN_PASSWORD_LENGTH} სიმბოლოსგან`;
    } else if (oldPassword && newPassword === oldPassword) {
      errors.newPassword = 'ახალი პაროლი არ უნდა იყოს ძველის იდენტური';
    }

    if (!errors.newPassword && newPassword !== confirmNewPassword) {
      errors.confirmNewPassword = 'ახალი პაროლები არ ემთხვევა ერთმანეთს';
    }

    this.passwordErrors = errors;
    return Object.keys(errors).length === 0;
  }

  savePassword(): void {
    if (!this.validatePasswordForm()) return;

    const userId = this.authService.getCurrentUser()?.userId;
    if (userId == null) return;

    this.passwordSaving = true;
    this.profileService.changePassword(userId, this.passwordForm).subscribe({
      next: () => {
        this.passwordSaving = false;
        this.toastService.showSuccess('პაროლი წარმატებით შეიცვალა');
        // Clear the fields but leave the modal open, per spec — the user
        // closes it themselves when they're done.
        this.resetPasswordForm();
      },
      error: (err: HttpErrorResponse) => {
        this.passwordSaving = false;
        // The backend returns a distinct { field, message } for each failure
        // case (wrong old password, mismatch, too weak, same as old) — show
        // it right under the relevant field instead of a generic toast.
        const field: string | undefined = err.error?.field;
        const message: string = err.error?.message || 'პაროლის შეცვლა ვერ მოხერხდა';
        if (field === 'oldPassword' || field === 'newPassword' || field === 'confirmNewPassword') {
          this.passwordErrors = { ...this.passwordErrors, [field]: message };
        } else {
          this.toastService.showError(message);
        }
      }
    });
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
    if (this.profileModalOpen) {
      this.closeProfileModal();
    } else if (this.draftService.isConfirmDialogVisible) {
      this.draftService.resolveConfirmDialog(false);
    } else if (this.draftService.isModalOpen) {
      this.draftService.closeAndDiscard();
    }
  }
}
