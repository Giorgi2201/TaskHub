// ============================================================
// PATCH: admin.component.ts
// Follow each numbered step below.
// ============================================================


// --- STEP 1: Add DigestItem interface (alongside your other interfaces at the top) ---

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


// --- STEP 2: Extend activeTab union type ---
// Find the line that looks like:
//   activeTab: 'users' | 'news' | 'vacancies' = 'users';
// Change it to:
//   activeTab: 'users' | 'news' | 'vacancies' | 'digest' = 'users';


// --- STEP 3: Add these class properties (alongside your other array/object properties) ---

digestEntries: DigestItem[] = [];

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


// --- STEP 4: Inside ngOnInit(), add this call alongside the others ---

this.loadDigestEntries();


// --- STEP 5: Inside closeModal(), add this reset block alongside the others ---

this.digestFormData = {
  digestEntryID: 0, title: '', description: '', imageUrl: '',
  sourceName: '', sourceUrl: '', periodFrom: '', periodTo: '',
  isFeatured: false, isActive: true, authorName: '', createdAt: ''
};
this.digestPhotoMode = 'url';
this.digestUploadedFile = null;


// --- STEP 6: Add these methods to the class body ---

loadDigestEntries(): void {
  this.userService.getDigestEntries().subscribe({
    next: (entries) => { this.digestEntries = entries; },
    error: (error) => { console.error('Error loading digest entries:', error); }
  });
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
