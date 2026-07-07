import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subject, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { UserService } from './user.service';
import { ToastService } from './toast.service';

// The set of admin modules that support minimizable drafts. Users is
// deliberately excluded (see planning discussion): its "create" flow
// generates credentials server-side rather than being a free-form draft.
export type ModuleType = 'digest' | 'vacancy' | 'news';

// Shape of each module's create/edit form. Mirrors the real entity's fields plus
// bookkeeping (id/`authorName`/`createdAt`) so a list-row item from AdminComponent
// can be passed straight into `openDraftModal(moduleType, entry)`.
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

export interface VacancyFormData {
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

export interface NewsFormData {
  newsID: number;
  title: string;
  content: string;
  imageUrl?: string;
  authorName: string;
  createdAt: string;
}

// Vacancy category options, needed by the vacancy modal's category <select>.
// Lives here (not in AdminComponent) since the vacancy modal itself now
// renders globally via AppComponent, same as digest's.
export const VACANCY_CATEGORIES = ['ღია საჯარო კონკურსი', 'შიდა კონკურსი', 'სააპლიკაციო ფორმა'];

function emptyDigestForm(): DigestFormData {
  return {
    digestEntryID: 0, title: '', description: '', imageUrl: '',
    sourceName: '', sourceUrl: '', periodFrom: '', periodTo: '',
    isFeatured: false, isActive: true, authorName: '', createdAt: ''
  };
}

function emptyVacancyForm(): VacancyFormData {
  return {
    vacancyID: 0, title: '', category: '', department: '', location: '',
    description: '', deadline: '', isActive: true, authorName: '', createdAt: ''
  };
}

function emptyNewsForm(): NewsFormData {
  return { newsID: 0, title: '', content: '', imageUrl: '', authorName: '', createdAt: '' };
}

// Produces a fresh, empty form object for a given module.
const EMPTY_FORM_FACTORIES: Record<ModuleType, () => Record<string, any>> = {
  digest: emptyDigestForm,
  vacancy: emptyVacancyForm,
  news: emptyNewsForm
};

// Which field on a module's published entity holds its identity, used by the
// "reopening the same entry that's already minimized" check in
// `openDraftModal`. Generalizes the old digest-only `entry.digestEntryID`
// comparison to work for any module.
const ENTRY_ID_FIELD: Record<ModuleType, string> = {
  digest: 'digestEntryID',
  vacancy: 'vacancyID',
  news: 'newsID'
};

// Per-module minimize/draft UI + persistence state. Keeping one of these per
// ModuleType (rather than one set of fields on the service itself, as the old
// digest-only version did) is what lets a user have, say, a digest draft AND
// a vacancy draft minimized at the same time, while still guaranteeing at
// most ONE minimized draft per module (there's exactly one state object per
// key, so there's no code path that could create a second one for the same
// module).
interface ModuleDraftState {
  minimized: boolean;
  modalOpen: boolean;
  editMode: boolean;
  // 0 = drafting a brand-new entry; nonzero = editing an existing published
  // entity with this id (see ENTRY_ID_FIELD). Tracked separately from
  // formData rather than read off it, since every module's id field is named
  // differently.
  entryId: number;
  formData: Record<string, any>;
  // Photo-upload UI state. Only digest uses this today, but it's per-module
  // so any future module with an image field (e.g. News) can reuse it as-is.
  photoMode: 'url' | 'upload';
  uploadedFile: File | null;
}

function emptyState(moduleType: ModuleType): ModuleDraftState {
  return {
    minimized: false,
    modalOpen: false,
    editMode: false,
    entryId: 0,
    formData: EMPTY_FORM_FACTORIES[moduleType](),
    photoMode: 'url',
    uploadedFile: null
  };
}

const ALL_MODULE_TYPES: ModuleType[] = ['digest', 'vacancy', 'news'];

// One row of the cross-module minimized-drafts summary used by the global
// bubble/panel in AppComponent (see `minimizedDrafts$` below).
export interface MinimizedDraftSummary {
  moduleType: ModuleType;
  title: string;
}

// Generalized "minimizable draft" feature for admin modules: owns the
// create/edit modal state, the minimize/restore/discard lifecycle, and the
// backend draft persistence (GET all / GET one / UPSERT / DELETE via the
// generic DraftsController) for every module type independently.
//
// This replaces the old digest-only DigestDraftService. The core state
// (minimized/modalOpen/editMode/formData/entryId per module) and the
// minimize/restore/saveDraft/discardDraft methods are fully generic and
// parameterized by `ModuleType`, and are what Digest, Vacancy, and News all
// share. Digest additionally keeps a few convenience passthrough properties
// (`formData`, `digestModalOpen$`, etc.) purely for template ergonomics on
// its own modal; Vacancy/News bind to the generic `state$()`/`getFormData()`
// API directly instead of growing their own set of passthroughs, to avoid
// the boilerplate multiplying per module.
//
// WHY THIS LIVES IN A DEDICATED SERVICE (not AppComponent or AdminComponent):
// - Every module's modal must render OUTSIDE <router-outlet> (in AppComponent's
//   template) so they stay visible/interactive across every route with no
//   navigation required to reopen them. But AppComponent already owns unrelated
//   app-shell concerns (auth state, nav, header visibility, user dropdown) —
//   bolting a full modal/draft lifecycle onto it would turn it into a dumping
//   ground.
// - AdminComponent still triggers "add"/"edit" from each tab and still owns
//   each module's ENTRIES table/list, but it no longer needs to know how
//   drafts are minimized, restored, or persisted.
// A single injectable service keeps this feature isolated, testable, and
// reusable regardless of which module or component happens to trigger/render it.
@Injectable({ providedIn: 'root' })
export class DraftService {
  // Talks to the generic DraftsController: GET all drafts, GET/PUT/DELETE one
  // draft per (userId, moduleType).
  private draftApiUrl = 'http://localhost:5250/api/drafts';

  // One BehaviorSubject per module type, created up front so every ModuleType
  // always has well-defined state (no "is this module initialized yet" checks
  // needed anywhere else in the service).
  private stateSubjects = new Map<ModuleType, BehaviorSubject<ModuleDraftState>>(
    ALL_MODULE_TYPES.map(m => [m, new BehaviorSubject<ModuleDraftState>(emptyState(m))])
  );

  // Fires after a module's real entity is successfully created/updated so any
  // interested component can refresh - namely AdminComponent's lists, since
  // saves can now be triggered from a globally-rendered modal instead of the
  // admin page itself. Kept as a single stream (not per-module) since nothing
  // currently needs to distinguish; consumers can inspect `moduleType` if
  // that ever changes.
  private savedSubject = new Subject<ModuleType>();
  saved$ = this.savedSubject.asObservable();
  // Back-compat alias for the one caller (AdminComponent) that only ever
  // cared about digest saves.
  digestSaved$: Observable<void> = this.saved$.pipe(map(() => undefined));

  // Themed "are you sure?" dialog for destructive draft actions (replaces the
  // native browser confirm() so it matches the rest of the site's visual design).
  // Shared across modules (only one modal/dialog can be on screen at a time),
  // but remembers which module it's currently guarding.
  private confirmDialogSubject = new BehaviorSubject<boolean>(false);
  confirmDialogVisible$ = this.confirmDialogSubject.asObservable();
  confirmDialogMessage = '';
  private pendingConfirmResolve: ((confirmed: boolean) => void) | null = null;
  private pendingModuleType: ModuleType | null = null;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private userService: UserService,
    private toastService: ToastService
  ) {}

  get isConfirmDialogVisible(): boolean { return this.confirmDialogSubject.value; }

  // ── Generic per-module state access ─────────────────────────────────

  private stateSubject(moduleType: ModuleType): BehaviorSubject<ModuleDraftState> {
    return this.stateSubjects.get(moduleType)!;
  }

  private getState(moduleType: ModuleType): ModuleDraftState {
    return this.stateSubject(moduleType).value;
  }

  private patchState(moduleType: ModuleType, patch: Partial<ModuleDraftState>): void {
    const subject = this.stateSubject(moduleType);
    subject.next({ ...subject.value, ...patch });
  }

  // Cached per module so templates that call `state$(moduleType)` directly
  // (e.g. the vacancy/news modals) get back the SAME Observable reference on
  // every change-detection cycle - otherwise `| async` would see a "new"
  // observable each time (since `.asObservable()` wraps a fresh object) and
  // needlessly unsubscribe/resubscribe on every check.
  private stateObservables = new Map<ModuleType, Observable<ModuleDraftState>>();

  state$(moduleType: ModuleType): Observable<ModuleDraftState> {
    let obs = this.stateObservables.get(moduleType);
    if (!obs) {
      obs = this.stateSubject(moduleType).asObservable();
      this.stateObservables.set(moduleType, obs);
    }
    return obs;
  }

  isMinimized(moduleType: ModuleType): boolean { return this.getState(moduleType).minimized; }
  isModalOpen(moduleType: ModuleType): boolean { return this.getState(moduleType).modalOpen; }
  isEditMode(moduleType: ModuleType): boolean { return this.getState(moduleType).editMode; }
  getFormData(moduleType: ModuleType): Record<string, any> { return this.getState(moduleType).formData; }

  // ── Cross-module summary (for the global minimized-drafts bubble) ───
  // Derived entirely from the generic per-module state map above - callers
  // (e.g. AppComponent's bubble) just iterate this list and never need to
  // know which specific module types exist or special-case any of them.
  // Every module's form data happens to use a `title` field (digest/vacancy/
  // news list items all do), so this stays generic rather than needing a
  // per-module "how do I get this draft's display label" mapping.
  // Defined once as a field (not a getter) so `| async` in the template
  // always sees the same Observable reference instead of resubscribing to a
  // freshly-built `combineLatest` on every change-detection cycle.
  minimizedDrafts$: Observable<MinimizedDraftSummary[]> = combineLatest(
    ALL_MODULE_TYPES.map(moduleType => this.state$(moduleType).pipe(
      map(state => ({ moduleType, minimized: state.minimized, title: (state.formData['title'] as string) || '' }))
    ))
  ).pipe(
    map(perModule => perModule
      .filter(m => m.minimized)
      .map(({ moduleType, title }) => ({ moduleType, title })))
  );

  minimizedCount$: Observable<number> = this.minimizedDrafts$.pipe(map(list => list.length));

  // Which module (if any) currently has its full modal open. Used by
  // AppComponent's Escape handler so it can close "whichever draft modal is
  // open" without hardcoding a check per module type.
  get openModuleType(): ModuleType | null {
    return ALL_MODULE_TYPES.find(m => this.getState(m).modalOpen) ?? null;
  }

  setPhotoMode(moduleType: ModuleType, mode: 'url' | 'upload'): void {
    const state = this.getState(moduleType);
    if (mode === 'upload') {
      state.formData['imageUrl'] = '';
    }
    this.patchState(moduleType, { photoMode: mode, uploadedFile: null });
  }

  onFileChange(moduleType: ModuleType, event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.patchState(moduleType, { uploadedFile: file });
      const reader = new FileReader();
      reader.onload = (e) => {
        this.getState(moduleType).formData['imageUrl'] = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  // ── Open / minimize / restore / discard (generic) ───────────────────

  // Open the modal for a module, either to create a new entry (entry = null)
  // or to edit an existing one (entry = the list-row object). If a draft for
  // THIS module is already minimized, warns the user first since proceeding
  // would silently overwrite/destroy it - unless they're re-opening the exact
  // same entry that's already minimized, in which case we just restore it as-is.
  async openDraftModal(moduleType: ModuleType, entry: Record<string, any> | null): Promise<void> {
    const entryId = entry ? (entry[ENTRY_ID_FIELD[moduleType]] ?? 0) : 0;
    const state = this.getState(moduleType);

    if (entry && state.minimized && state.entryId === entryId && entryId !== 0) {
      this.restore(moduleType);
      return;
    }

    if (!(await this.confirmDiscardMinimizedDraft(moduleType))) return;

    this.patchState(moduleType, {
      editMode: !!entry,
      formData: entry ? { ...entry } : EMPTY_FORM_FACTORIES[moduleType](),
      entryId,
      photoMode: 'url',
      uploadedFile: null,
      modalOpen: true
    });
  }

  // Minimize the modal into the floating bubble: hide the modal, flag as
  // minimized, and persist the current form as this user's draft for this
  // module so it also survives a page refresh.
  minimize(moduleType: ModuleType): void {
    this.patchState(moduleType, { modalOpen: false, minimized: true });
    this.saveDraft(moduleType);
  }

  // Bring a minimized draft back exactly as it was left. The form state is
  // already held in-memory by this service, so restore is instant and
  // requires no network round-trip or page navigation.
  restore(moduleType: ModuleType): void {
    this.patchState(moduleType, { minimized: false, modalOpen: true });
  }

  // Upsert the current form state as this user's single persisted draft for
  // this module, via the generic DraftsController (PUT .../{userId}/{moduleType}).
  saveDraft(moduleType: ModuleType): void {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser) return;

    const draftData = this.getState(moduleType).formData;
    this.http.put<any>(`${this.draftApiUrl}/${currentUser.userId}/${moduleType}`, { draftData }).subscribe({
      error: (error) => { console.error(`Error saving ${moduleType} draft:`, error); }
    });
  }

  // Delete this module's persisted draft (no-op-safe on the backend), reset
  // its in-memory state back to empty, and close its modal - there's nothing
  // left to show once the draft is gone. Generalizes both the old bare
  // `discardDraft()` (internal use) and `closeAndDiscard()` (template-facing
  // cancel/close action) into one method, since after a discard the modal
  // should always end up closed either way.
  discardDraft(moduleType: ModuleType): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.http.delete<void>(`${this.draftApiUrl}/${currentUser.userId}/${moduleType}`).subscribe({
        error: (error) => { console.error(`Error deleting ${moduleType} draft:`, error); }
      });
    }
    this.stateSubject(moduleType).next(emptyState(moduleType));
  }

  // If a draft is currently minimized FOR THIS MODULE, warn the user via the
  // themed confirm dialog that continuing will discard it. Resolves true when
  // it's safe for the caller to proceed with opening a fresh add/edit session.
  // Scoped per-module: minimizing a digest draft never blocks opening a
  // vacancy modal (or vice versa), since they're independent slots.
  private confirmDiscardMinimizedDraft(moduleType: ModuleType): Promise<boolean> {
    if (!this.isMinimized(moduleType)) return Promise.resolve(true);

    return new Promise<boolean>((resolve) => {
      this.confirmDialogMessage = 'თქვენ გაქვთ ჩაკეცილი დრაფტი. გაგრძელება წაშლის მას. გსურთ გაგრძელება?';
      this.pendingModuleType = moduleType;
      this.pendingConfirmResolve = resolve;
      this.confirmDialogSubject.next(true);
    });
  }

  // Called by the dialog's own buttons (AppComponent) once the user picks an
  // option. Hides the dialog and settles the promise `confirmDiscardMinimizedDraft`
  // is waiting on; if `confirmed`, also discards the pending module's draft.
  resolveConfirmDialog(confirmed: boolean): void {
    this.confirmDialogSubject.next(false);
    const moduleType = this.pendingModuleType;
    this.pendingModuleType = null;
    if (confirmed && moduleType) {
      this.discardDraft(moduleType);
    }
    const resolve = this.pendingConfirmResolve;
    this.pendingConfirmResolve = null;
    resolve?.(confirmed);
  }

  // ── App-wide init / reset ────────────────────────────────────────────

  // Load ALL of the user's persisted drafts (one request, across every
  // module) once the current user is known (called from AppComponent on
  // login/app init), so every module's bubble reappears app-wide after a
  // refresh, regardless of which route loads first.
  init(userId: number): void {
    this.http.get<{ moduleType: ModuleType; draftData: any; lastUpdated: string }[]>(`${this.draftApiUrl}/${userId}`).subscribe({
      next: (drafts) => {
        for (const draft of drafts) {
          const d = draft.draftData || {};
          this.patchState(draft.moduleType, {
            // No entry id is persisted server-side, so a restored draft
            // resumes as a new entry rather than an edit of a specific one.
            entryId: 0,
            formData: { ...EMPTY_FORM_FACTORIES[draft.moduleType](), ...d },
            editMode: false,
            minimized: true
          });
        }
      },
      error: (error) => { console.error('Error loading drafts:', error); }
    });
  }

  // Reset all modules' client-side state (e.g. on logout). Does not touch the
  // server drafts so they're still there to reload on the next login.
  reset(): void {
    for (const moduleType of ALL_MODULE_TYPES) {
      this.stateSubject(moduleType).next(emptyState(moduleType));
    }
  }

  // ── Digest-specific convenience layer ───────────────────────────────
  // The digest modal (app.component.html) binds directly to these properties/
  // observables via `[(ngModel)]` and `| async`. They're thin passthroughs
  // onto the generic 'digest' state above - not extra state of their own.
  // Vacancy/News modals bind to the generic `state$('vacancy'|'news')` and
  // `getFormData('vacancy'|'news')` directly instead (see app.component.html)
  // rather than duplicating this passthrough shape per module.
  get formData(): DigestFormData { return this.getState('digest').formData as DigestFormData; }
  get uploadedFile(): File | null { return this.getState('digest').uploadedFile; }
  digestModalOpen$ = this.state$('digest').pipe(map(s => s.modalOpen));
  digestModalMinimized$ = this.state$('digest').pipe(map(s => s.minimized));
  isEditMode$ = this.state$('digest').pipe(map(s => s.editMode));
  digestPhotoMode$ = this.state$('digest').pipe(map(s => s.photoMode));

  // Validate and create/update the REAL digest entry (the published digest table,
  // not the draft). On success, discard the draft/bubble and emit `saved$` so
  // interested components (AdminComponent's list) can refresh - necessary
  // because save can now be triggered from the globally-rendered modal instead
  // of from the admin page itself. This is digest-specific business logic
  // (different DTOs/services per module) rather than part of the generic
  // open/minimize/restore/saveDraft/discardDraft API.
  saveDigestEntry(): void {
    const data = this.getState('digest').formData as DigestFormData;
    if (!data.title || !data.description || !data.periodFrom || !data.periodTo) {
      this.toastService.showWarning('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode('digest')) {
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
        next: () => { this.discardDraft('digest'); this.savedSubject.next('digest'); this.toastService.showSuccess('ჩანაწერი განახლდა'); },
        error: () => { this.toastService.showError('შეცდომა ჩანაწერის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { this.toastService.showError('მომხმარებელი არ არის ავტორიზებული'); return; }

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
        next: () => { this.discardDraft('digest'); this.savedSubject.next('digest'); this.toastService.showSuccess('ჩანაწერი დაემატა'); },
        error: () => { this.toastService.showError('შეცდომა ჩანაწერის შექმნისას'); }
      });
    }
  }

  // Validate and create/update the REAL vacancy (not the draft). Same shape
  // as `saveDigestEntry` - module-specific business logic (its own DTOs/
  // UserService methods) rather than part of the generic draft API.
  saveVacancyEntry(): void {
    const data = this.getState('vacancy').formData as VacancyFormData;
    if (!data.title || !data.category || !data.description) {
      this.toastService.showWarning('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode('vacancy')) {
      this.userService.updateVacancy(data.vacancyID, data).subscribe({
        next: () => { this.discardDraft('vacancy'); this.savedSubject.next('vacancy'); this.toastService.showSuccess('ვაკანსია განახლდა'); },
        error: () => { this.toastService.showError('შეცდომა ვაკანსიის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { this.toastService.showError('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = { ...data, authorID: currentUser.userId };
      this.userService.createVacancy(createData).subscribe({
        next: () => { this.discardDraft('vacancy'); this.savedSubject.next('vacancy'); this.toastService.showSuccess('ვაკანსია დაემატა'); },
        error: () => { this.toastService.showError('შეცდომა ვაკანსიის შექმნისას'); }
      });
    }
  }

  // Validate and create/update the REAL news item (not the draft). Same
  // shape as `saveDigestEntry`/`saveVacancyEntry`.
  saveNewsEntry(): void {
    const data = this.getState('news').formData as NewsFormData;
    if (!data.title || !data.content) {
      this.toastService.showWarning('გთხოვთ შეავსოთ სავალდებულო ველები');
      return;
    }

    if (this.isEditMode('news')) {
      const updateData = { title: data.title, content: data.content, imageUrl: data.imageUrl || null };
      this.userService.updateNews(data.newsID, updateData).subscribe({
        next: () => { this.discardDraft('news'); this.savedSubject.next('news'); this.toastService.showSuccess('სიახლე განახლდა'); },
        error: () => { this.toastService.showError('შეცდომა სიახლის განახლებისას'); }
      });
    } else {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) { this.toastService.showError('მომხმარებელი არ არის ავტორიზებული'); return; }

      const createData = { title: data.title, content: data.content, imageUrl: data.imageUrl || null, authorID: currentUser.userId };
      this.userService.createNews(createData).subscribe({
        next: () => { this.discardDraft('news'); this.savedSubject.next('news'); this.toastService.showSuccess('სიახლე დაემატა'); },
        error: () => { this.toastService.showError('შეცდომა სიახლის შექმნისას'); }
      });
    }
  }
}
