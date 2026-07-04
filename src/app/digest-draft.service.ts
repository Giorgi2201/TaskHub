import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { UserService } from './user.service';

// Shape of the digest create/edit form. Mirrors the real digest entry fields plus
// bookkeeping (`digestEntryID`/`authorName`/`createdAt`) so a `DigestEntry` list item
// from AdminComponent can be passed straight into `openEditDigestModal()`.
export interface DigestFormData {
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

function emptyDigestForm(): DigestFormData {
  return {
    digestEntryID: 0, title: '', description: '', imageUrl: '',
    sourceName: '', sourceUrl: '', periodFrom: '', periodTo: '',
    isFeatured: false, isActive: true, authorName: '', createdAt: ''
  };
}

// Owns the ENTIRE "minimized digest draft" feature end to end: the digest
// create/edit modal's state, the minimize/restore/discard lifecycle, and the
// backend draft persistence (GET/UPSERT/DELETE), plus the real digest entry
// save (create/update) that the modal's own "save" action triggers.
//
// WHY THIS LIVES IN A DEDICATED SERVICE (not AppComponent or AdminComponent):
// - The bubble + modal must render OUTSIDE <router-outlet> (in AppComponent's
//   template) so they stay visible/interactive across every route with no
//   navigation required to reopen them. But AppComponent already owns unrelated
//   app-shell concerns (auth state, nav, header visibility, user dropdown) —
//   bolting a full modal/draft lifecycle onto it would turn it into a dumping
//   ground.
// - AdminComponent still triggers "add"/"edit" from its digest tab and still
//   owns the digest ENTRIES table/list, but it no longer needs to know how
//   drafts are minimized, restored, or persisted.
// A single injectable service keeps this feature isolated, testable, and
// reusable regardless of which component happens to trigger or render it.
@Injectable({ providedIn: 'root' })
export class DigestDraftService {
  private draftApiUrl = 'http://localhost:5250/api/digestdrafts';

  // Whether a draft is minimized into the floating bubble.
  private minimizedSubject = new BehaviorSubject<boolean>(false);
  digestModalMinimized$ = this.minimizedSubject.asObservable();

  // Whether the full digest modal is open. Deliberately a SEPARATE flag from
  // AdminComponent's own `modalOpen` (which only ever governs the users/news/
  // vacancies modals) so this modal can be rendered and controlled independently
  // from AppComponent without any risk of colliding with those other modals.
  private modalOpenSubject = new BehaviorSubject<boolean>(false);
  digestModalOpen$ = this.modalOpenSubject.asObservable();

  private formDataSubject = new BehaviorSubject<DigestFormData>(emptyDigestForm());
  digestFormData$ = this.formDataSubject.asObservable();

  private editModeSubject = new BehaviorSubject<boolean>(false);
  isEditMode$ = this.editModeSubject.asObservable();

  // Photo-mode UI state for the modal's image field. Lives here (not in
  // AppComponent) because it's specific to this feature's modal, not app-shell state.
  private photoModeSubject = new BehaviorSubject<'url' | 'upload'>('url');
  digestPhotoMode$ = this.photoModeSubject.asObservable();
  uploadedFile: File | null = null;

  // Fires after a digest entry is successfully created/updated so any interested
  // component can refresh — namely AdminComponent's entries list, since the save
  // can now be triggered from the globally-rendered modal instead of the admin page.
  private savedSubject = new Subject<void>();
  digestSaved$ = this.savedSubject.asObservable();

  // Themed "are you sure?" dialog for destructive draft actions (replaces the
  // native browser confirm() so it matches the rest of the site's visual design).
  // Rendered by AppComponent; this service only owns the message + the pending
  // promise resolver so callers can `await` the user's choice.
  private confirmDialogSubject = new BehaviorSubject<boolean>(false);
  confirmDialogVisible$ = this.confirmDialogSubject.asObservable();
  confirmDialogMessage = '';
  private pendingConfirmResolve: ((confirmed: boolean) => void) | null = null;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private userService: UserService
  ) {}

  get isMinimized(): boolean { return this.minimizedSubject.value; }
  get isModalOpen(): boolean { return this.modalOpenSubject.value; }
  get isConfirmDialogVisible(): boolean { return this.confirmDialogSubject.value; }
  get formData(): DigestFormData { return this.formDataSubject.value; }
  get isEditMode(): boolean { return this.editModeSubject.value; }
  get photoMode(): 'url' | 'upload' { return this.photoModeSubject.value; }

  setPhotoMode(mode: 'url' | 'upload'): void {
    this.photoModeSubject.next(mode);
    this.uploadedFile = null;
    if (mode === 'upload') {
      this.formData.imageUrl = '';
    }
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.uploadedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = (e) => {
        this.formData.imageUrl = e.target?.result as string;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }

  // Open the modal for a brand-new digest entry. If a draft is already
  // minimized, warns the user first since proceeding would silently destroy it.
  async openAddDigestModal(): Promise<void> {
    if (!(await this.confirmDiscardMinimizedDraft())) return;
    this.editModeSubject.next(false);
    this.formDataSubject.next(emptyDigestForm());
    this.photoModeSubject.next('url');
    this.uploadedFile = null;
    this.modalOpenSubject.next(true);
  }

  // Open the modal to edit an existing entry. Same minimized-draft guard as add.
  async openEditDigestModal(entry: DigestFormData): Promise<void> {
    if (!(await this.confirmDiscardMinimizedDraft())) return;
    this.editModeSubject.next(true);
    this.formDataSubject.next({ ...entry });
    this.photoModeSubject.next('url');
    this.uploadedFile = null;
    this.modalOpenSubject.next(true);
  }

  // If a draft is currently minimized, warn the user via the themed confirm
  // dialog (see `confirmDialogVisible$`) that continuing will discard it.
  // Resolves true when it's safe for the caller to proceed with opening a
  // fresh add/edit session.
  private confirmDiscardMinimizedDraft(): Promise<boolean> {
    if (!this.isMinimized) return Promise.resolve(true);

    return new Promise<boolean>((resolve) => {
      this.confirmDialogMessage = 'თქვენ გაქვთ ჩაკეცილი დაიჯესტის დრაფტი. გაგრძელება წაშლის მას. გსურთ გაგრძელება?';
      this.pendingConfirmResolve = resolve;
      this.confirmDialogSubject.next(true);
    });
  }

  // Called by the dialog's own buttons (AppComponent) once the user picks an
  // option. Hides the dialog and settles the promise `confirmDiscardMinimizedDraft`
  // is waiting on; if `confirmed`, also discards the draft before resuming.
  resolveConfirmDialog(confirmed: boolean): void {
    this.confirmDialogSubject.next(false);
    if (confirmed) {
      this.discardDraft();
    }
    const resolve = this.pendingConfirmResolve;
    this.pendingConfirmResolve = null;
    resolve?.(confirmed);
  }

  // Minimize the modal into the floating bubble: hide the modal, flag as
  // minimized, and persist the current form as this user's draft so it also
  // survives a page refresh.
  minimize(): void {
    this.modalOpenSubject.next(false);
    this.minimizedSubject.next(true);
    this.saveDraft();
  }

  // Bring a minimized draft back exactly as it was left. The form state is
  // already held in-memory by this service, so restore is instant and
  // requires no network round-trip or page navigation.
  restore(): void {
    this.minimizedSubject.next(false);
    this.modalOpenSubject.next(true);
  }

  // Explicit close/cancel (as opposed to minimize): tear down the modal and
  // bubble, delete the persisted draft, and reset the form to its default state.
  closeAndDiscard(): void {
    this.discardDraft();
    this.modalOpenSubject.next(false);
  }

  // Delete this user's persisted draft (no-op-safe on the backend) and reset
  // all in-memory draft/modal state.
  discardDraft(): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.http.delete<void>(`${this.draftApiUrl}/${currentUser.userId}`).subscribe({
        error: (error) => { console.error('Error deleting digest draft:', error); }
      });
    }
    this.minimizedSubject.next(false);
    this.formDataSubject.next(emptyDigestForm());
    this.editModeSubject.next(false);
    this.photoModeSubject.next('url');
    this.uploadedFile = null;
  }

  // Upsert the current form state as this user's single persisted draft.
  saveDraft(): void {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser) return;

    const data = this.formDataSubject.value;
    const payload = {
      userID: currentUser.userId,
      title: data.title,
      description: data.description,
      imageUrl: data.imageUrl || null,
      sourceName: data.sourceName,
      sourceUrl: data.sourceUrl,
      // Empty date inputs are sent as null so model binding doesn't choke on ''.
      periodFrom: data.periodFrom || null,
      periodTo: data.periodTo || null,
      isFeatured: data.isFeatured,
      isActive: data.isActive
    };
    this.http.post<any>(this.draftApiUrl, payload).subscribe({
      error: (error) => { console.error('Error saving digest draft:', error); }
    });
  }

  // Load the persisted draft once the current user is known (called from
  // AppComponent on login/app init) so the bubble reappears app-wide after a
  // refresh, regardless of which route loads first. A 204 response yields a
  // null body, meaning "no draft" — nothing is shown in that case.
  init(userId: number): void {
    this.http.get<any>(`${this.draftApiUrl}/${userId}`).subscribe({
      next: (d) => {
        if (!d) return;
        this.formDataSubject.next({
          // No entry id is persisted server-side, so a restored draft resumes
          // as a new entry rather than an edit of a specific one.
          digestEntryID: 0,
          title: d.title || '',
          description: d.description || '',
          imageUrl: d.imageUrl || '',
          sourceName: d.sourceName || '',
          sourceUrl: d.sourceUrl || '',
          periodFrom: d.periodFrom || '',
          periodTo: d.periodTo || '',
          isFeatured: !!d.isFeatured,
          isActive: d.isActive ?? true,
          authorName: '',
          createdAt: ''
        });
        this.editModeSubject.next(false);
        this.minimizedSubject.next(true);
      },
      error: (error) => { console.error('Error loading digest draft:', error); }
    });
  }

  // Reset all client-side state (e.g. on logout). Does not touch the server draft
  // so it's still there to reload on the next login.
  reset(): void {
    this.minimizedSubject.next(false);
    this.modalOpenSubject.next(false);
    this.formDataSubject.next(emptyDigestForm());
    this.editModeSubject.next(false);
    this.photoModeSubject.next('url');
    this.uploadedFile = null;
  }

  // Validate and create/update the REAL digest entry (the published digest table,
  // not the draft). On success, discard the draft/bubble and emit `digestSaved$`
  // so interested components (AdminComponent's list) can refresh — necessary
  // because save can now be triggered from the globally-rendered modal instead
  // of from the admin page itself.
  saveDigestEntry(): void {
    const data = this.formDataSubject.value;
    if (!data.title || !data.description || !data.periodFrom || !data.periodTo) {
      alert('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.editModeSubject.value) {
      const updateData = {
        title: data.title,
        description: data.description,
        imageUrl: data.imageUrl || null,
        sourceName: data.sourceName,
        sourceUrl: data.sourceUrl,
        periodFrom: data.periodFrom,
        periodTo: data.periodTo,
        isFeatured: data.isFeatured,
        isActive: data.isActive
      };
      this.userService.updateDigestEntry(data.digestEntryID, updateData).subscribe({
        next: () => { this.closeAndDiscard(); this.savedSubject.next(); },
        error: (error) => { console.error('Error updating digest entry:', error); alert('შეცდომა ჩანაწერის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { alert('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = {
        title: data.title,
        description: data.description,
        imageUrl: data.imageUrl || null,
        sourceName: data.sourceName,
        sourceUrl: data.sourceUrl,
        periodFrom: data.periodFrom,
        periodTo: data.periodTo,
        isFeatured: data.isFeatured,
        isActive: data.isActive,
        authorID: currentUser.userId
      };
      this.userService.createDigestEntry(createData).subscribe({
        next: () => { this.closeAndDiscard(); this.savedSubject.next(); },
        error: (error) => { console.error('Error creating digest entry:', error); alert('შეცდომა ჩანაწერის შექმნისას'); }
      });
    }
  }
}
