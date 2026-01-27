import { Component, OnInit } from '@angular/core';
import { RouterOutlet, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RequestService, RequestDetail } from '../request.service';
import { UserService } from '../user.service';

@Component({
  selector: 'app-request-detail',
  imports: [RouterOutlet, CommonModule, FormsModule],
  templateUrl: './request-detail.component.html',
  styleUrl: './request-detail.component.css'
})
export class RequestDetailComponent implements OnInit {
  requestData: RequestDetail | null = null;
  currentUserRole = '';
  commentText = '';
  requestId = 0;

  constructor(
    private router: Router, 
    private route: ActivatedRoute,
    private requestService: RequestService,
    private userService: UserService
  ) {}

  ngOnInit() {
    this.userService.currentUser$.subscribe(user => {
      this.currentUserRole = user.role;
    });

    this.route.params.subscribe(params => {
      const id = params['id'];
      this.requestId = +id;
      this.loadRequest();
    });
  }

  loadRequest() {
    this.requestService.getRequest(this.requestId).subscribe({
      next: (data) => this.requestData = data,
      error: (err) => {
        console.error('Error loading request:', err);
        this.router.navigate(['/requests']);
      }
    });
  }

  get canApproveOrReject(): boolean {
    return this.currentUserRole === 'ხელმძღვანელი' && 
           this.requestData?.statusClass === 'pending';
  }

  approveRequest() {
    const user = this.userService.getCurrentUser();
    this.requestService.approveRequest(this.requestId, user.userId, this.commentText).subscribe({
      next: () => {
        this.commentText = '';
        this.loadRequest(); // Reload to get updated data
      },
      error: (err) => console.error('Error approving request:', err)
    });
  }

  rejectRequest() {
    if (!this.commentText.trim()) {
      alert('უარყოფისთვის აუცილებელია კომენტარის დამატება');
      return;
    }

    const user = this.userService.getCurrentUser();
    this.requestService.rejectRequest(this.requestId, user.userId, this.commentText).subscribe({
      next: () => {
        this.commentText = '';
        this.loadRequest(); // Reload to get updated data
      },
      error: (err) => console.error('Error rejecting request:', err)
    });
  }

  goBack() {
    this.router.navigate(['/requests']);
  }

  getCurrentStatusId(): number {
    // Map status class to status ID
    const statusMap: { [key: string]: number } = {
      'pending': 2,
      'approved': 3,
      'inprogress': 4,
      'completed': 5,
      'rejected': 6
    };
    return this.requestData?.statusClass ? statusMap[this.requestData.statusClass] : 0;
  }

  isStatusCompleted(statusId: number): boolean {
    const currentStatus = this.getCurrentStatusId();
    // A status is completed if current status is greater than it
    return currentStatus > statusId;
  }

  isStatusCurrent(statusId: number): boolean {
    const currentStatus = this.getCurrentStatusId();
    // A status is current if it matches the current status
    return currentStatus === statusId;
  }

  isRejected(): boolean {
    return this.getCurrentStatusId() === 6;
  }
}
