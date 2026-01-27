import { Component, OnInit, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RequestService, Request } from '../request.service';
import { UserService } from '../user.service';

@Component({
  selector: 'app-request-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './request-list.component.html',
  styleUrl: './request-list.component.css'
})
export class RequestListComponent implements OnInit {
  requests: Request[] = [];
  searchTerm = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 5;

  // Status filter
  statusDropdownOpen = false;
  selectedStatusFilter: number | null = null;

  // Edit modal
  editModalOpen = false;
  selectedRequest: Request | null = null;
  currentUserRole = '';

  // Edit form
  selectedExecutorId: number | null = null;
  selectedStatusId: number | null = null;
  editComment = '';
  executors: Array<{id: number, name: string}> = [];

  statusOptions = [
    { id: null, label: 'ყველა სტატუსი' },
    { id: 2, label: 'დამტკიცების მოლოდინში' },
    { id: 3, label: 'დამტკიცებული' },
    { id: 4, label: 'შესრულების პროცესში' },
    { id: 5, label: 'შესრულებული' },
    { id: 6, label: 'უარყოფილი' }
  ];

  editStatusOptions = [
    { id: 3, label: 'დამტკიცებული' },
    { id: 4, label: 'შესრულების პროცესში' },
    { id: 5, label: 'შესრულებული' }
  ];

  constructor(
    private router: Router, 
    private requestService: RequestService,
    private userService: UserService
  ) {}

  ngOnInit() {
    this.userService.currentUser$.subscribe(user => {
      this.currentUserRole = user.role;
    });
    this.loadRequests();
    this.loadExecutors();
  }

  loadExecutors() {
    this.requestService.getExecutors().subscribe({
      next: (data) => {
        this.executors = data.map(e => ({ id: e.userID, name: e.name }));
      },
      error: (err) => console.error('Error loading executors:', err)
    });
  }

  formatRequestId(id: number | undefined): string {
    return (id || 0).toString().padStart(4, '0');
  }

  loadRequests() {
    this.requestService.getRequests().subscribe({
      next: (data) => this.requests = data,
      error: (err) => console.error(err)
    });
  }

  get filteredRequests() {
    let filtered = this.requests;

    // Filter by search term (description only)
    if (this.searchTerm.trim()) {
      filtered = filtered.filter(r => 
        r.description.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }

    // Filter by status
    if (this.selectedStatusFilter !== null) {
      filtered = filtered.filter(r => r.statusID === this.selectedStatusFilter);
    }

    return filtered;
  }

  get paginatedRequests() {
    const filtered = this.filteredRequests;
    const startIndex = (this.currentPage - 1) * this.pageSize;
    const endIndex = startIndex + this.pageSize;
    return filtered.slice(startIndex, endIndex);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredRequests.length / this.pageSize);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  get startIndex(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredRequests.length);
  }

  toggleStatusDropdown() {
    this.statusDropdownOpen = !this.statusDropdownOpen;
  }

  selectStatus(statusId: number | null) {
    this.selectedStatusFilter = statusId;
    this.statusDropdownOpen = false;
    this.currentPage = 1; // Reset to first page when filtering
  }

  getSelectedStatusLabel(): string {
    if (this.selectedStatusFilter === null) {
      return 'ყველა სტატუსი';
    }
    const status = this.statusOptions.find(s => s.id === this.selectedStatusFilter);
    return status ? status.label : 'ყველა სტატუსი';
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    const filterDropdown = target.closest('.filter-dropdown');
    
    if (!filterDropdown && this.statusDropdownOpen) {
      this.statusDropdownOpen = false;
    }
  }

  // Maps the ID to Georgian status text
  getStatusLabel(id: number) {
    switch(id) {
      case 1: return 'ახალი';
      case 2: return 'დამტკიცების მოლოდინში';
      case 3: return 'დამტკიცებული';
      case 4: return 'შესრულების პროცესში';
      case 5: return 'შესრულებული';
      case 6: return 'უარყოფილი';
      default: return 'ახალი';
    }
  }

  getStatusClass(id: number) {
    switch(id) {
      case 1: return 'new';
      case 2: return 'pending';
      case 3: return 'approved';
      case 4: return 'inprogress';
      case 5: return 'completed';
      case 6: return 'rejected';
      default: return 'new';
    }
  }

  onSearchChange(event: Event) {
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.currentPage = 1; // Reset to first page when searching
  }

  canEditRequest(statusId: number): boolean {
    return this.currentUserRole === 'ხელმძღვანელი' && (statusId === 3 || statusId === 4);
  }

  openEditModal(request: Request) {
    this.selectedRequest = request;
    this.selectedStatusId = request.statusID;
    this.selectedExecutorId = null;
    this.editComment = '';
    this.editModalOpen = true;
  }

  closeEditModal() {
    this.editModalOpen = false;
    this.selectedRequest = null;
    this.selectedExecutorId = null;
    this.selectedStatusId = null;
    this.editComment = '';
  }

  saveRequestUpdate() {
    if (!this.selectedRequest) return;
    
    if (!this.selectedStatusId) {
      alert('გთხოვთ აირჩიოთ სტატუსი');
      return;
    }

    const user = this.userService.getCurrentUser();
    
    this.requestService.updateRequestStatus(
      this.selectedRequest.requestID!,
      this.selectedStatusId,
      this.selectedExecutorId,
      user.userId,
      this.editComment
    ).subscribe({
      next: () => {
        this.closeEditModal();
        this.loadRequests();
      },
      error: (err) => console.error('Error updating request:', err)
    });
  }

  viewRequest(id?: number) {
    if (id) this.router.navigate(['/request', id]);
  }

  createNewRequest() {
    this.router.navigate(['/request/new']);
  }
}