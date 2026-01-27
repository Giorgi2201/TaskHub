import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RequestService, Category } from '../request.service';

interface CategoryWithIcon {
  name: string;
  icon: string;
}

@Component({
  selector: 'app-request-new',
  imports: [CommonModule],
  templateUrl: './request-new.component.html',
  styleUrl: './request-new.component.css'
})
export class RequestNewComponent implements OnInit {
  categories: CategoryWithIcon[] = [];

  private categoryIcons: { [key: string]: string } = {
    'კომპიუტერული ტექნიკა': 'computer',
    'პრინტერი': 'printer',
    'ქსელი': 'network',
    'პროგრამული უზრუნველყოფა': 'software',
    'ტელეფონი': 'phone',
    'სხვა': 'other'
  };

  constructor(
    private router: Router,
    private requestService: RequestService
  ) {}

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.requestService.getCategories().subscribe({
      next: (data) => {
        this.categories = data.map(cat => ({
          name: cat.categoryName,
          icon: this.categoryIcons[cat.categoryName] || 'other'
        }));
      },
      error: (err) => console.error('Error loading categories:', err)
    });
  }

  selectCategory(category: string) {
    this.router.navigate(['/request/new', category]);
  }
}
