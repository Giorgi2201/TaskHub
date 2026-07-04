import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { RequestService, Request } from '../request.service';
import { AuthService } from '../auth.service';
import { ToastService } from '../toast.service';

@Component({
  selector: 'app-request-form',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './request-form.component.html',
  styleUrl: './request-form.component.css'
})
export class RequestFormComponent implements OnInit {
  // Variables referenced in HTML
  categoryName: string = '';
  subcategories: string[] = [];
  selectedSubcategory: string = '';
  description: string = '';
  requestNumber: string = '';
  currentDate: string = '';
  warningMessage: string = '';
  showWarning: boolean = false;
  isSubmitting: boolean = false;

  // Loading state for subcategories
  loading = true;

  // Initiator data from logged-in user
  initiatorName: string = '';
  initiatorEmail: string = '';
  initiatorPhone: string = '';
  initiatorTitle: string = '';
  initiatorDepartment: string = '';
  initiatorId: number = 0;

  constructor(
    private router: Router, 
    private route: ActivatedRoute,
    private requestService: RequestService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.categoryName = params['category'] || '';
      this.loadSubcategories();
      this.generateTempNumber();
      this.setCurrentDate();
      this.loadInitiatorData();
    });
  }

  setCurrentDate() {
    const today = new Date();
    const day = today.getDate();
    const month = today.getMonth() + 1;
    const year = today.getFullYear();
    this.currentDate = `${day}/${month}/${year}`;
  }

  loadInitiatorData() {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser) {
      this.initiatorName = currentUser.name;
      this.initiatorEmail = currentUser.email;
      this.initiatorPhone = currentUser.phone;
      this.initiatorTitle = currentUser.title;
      this.initiatorDepartment = currentUser.department;
      this.initiatorId = currentUser.userId;
    }
  }

  loadSubcategories() {
    this.loading = true;
    this.requestService.getCategories().subscribe({
      next: (categories) => {
        const category = categories.find(c => c.categoryName === this.categoryName);
        if (category) {
          this.subcategories = category.subcategories.map(s => s.subcategoryName);
        }
        this.loading = false;
      },
      error: (err) => { console.error('Error loading subcategories:', err); this.loading = false; this.toastService.showError('ქვეკატეგორიების ჩატვირთვა ვერ მოხერხდა'); }
    });
  }

  generateTempNumber() {
    // Generate a temporary draft number like #Draft-1234
    this.requestNumber = '#DRAFT-' + Math.floor(Math.random() * 9000 + 1000);
  }

  onSubcategoryChange(event: Event) {
    this.selectedSubcategory = (event.target as HTMLSelectElement).value;
    this.showWarning = false;
  }

  onDescriptionChange(event: Event) {
    this.description = (event.target as HTMLTextAreaElement).value;
    this.showWarning = false;
  }

  submitRequest(event: Event) {
    event.preventDefault();
    this.showWarning = false;

    // Validation
    if (!this.selectedSubcategory) {
      this.warningMessage = 'გთხოვთ აირჩიოთ ქვეკატეგორია';
      this.showWarning = true;
      return;
    }
    if (!this.description || this.description.trim().length < 5) {
      this.warningMessage = 'გთხოვთ შეიყვანოთ აღწერა (მინ. 5 სიმბოლო)';
      this.showWarning = true;
      return;
    }

    this.isSubmitting = true;

    // Prepare data for C# Backend
    const newRequest: Request = {
      category: this.categoryName,
      subcategory: this.selectedSubcategory,
      description: this.description,
      statusID: 2, // 2 = "დამტკიცების მოლოდინში" in our SQL Statuses table
      initiatorID: this.initiatorId
    };

    this.requestService.createRequest(newRequest).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toastService.showSuccess('მოთხოვნა წარმატებით გაიგზავნა');
        this.router.navigate(['/requests']);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.warningMessage = 'კავშირის შეცდომა: დარწმუნდით რომ Backend ჩართულია';
        this.showWarning = true;
        console.error(err);
        this.toastService.showError('მოთხოვნის გაგზავნა ვერ მოხერხდა');
      }
    });
  }

  goBack() {
    this.router.navigate(['/request/new']);
  }
}