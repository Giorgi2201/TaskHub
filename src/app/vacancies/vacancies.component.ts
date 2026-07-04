import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../user.service';

interface Vacancy {
  vacancyID: number;
  title: string;
  category: string;
  department: string;
  location: string;
  description: string;
  deadline?: string;
  isActive: boolean;
  authorName: string;
  createdAt: string;
}

@Component({
  selector: 'app-vacancies',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vacancies.component.html',
  styleUrl: './vacancies.component.css'
})
export class VacanciesComponent implements OnInit {
  selectedCategory = 'ყველა';
  allVacancies: Vacancy[] = [];
  isLoading = true;

  categories = [
    { label: 'ყველა ვაკანსია', value: 'ყველა' },
    { label: 'სააპლიკაციო ფორმა', value: 'სააპლიკაციო ფორმა' },
    { label: 'ღია საჯარო კონკურსი', value: 'ღია საჯარო კონკურსი' },
    { label: 'შიდა კონკურსი', value: 'შიდა კონკურსი' },
  ];

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadVacancies();
  }

  loadVacancies(): void {
    this.isLoading = true;
    this.userService.getVacancies(true).subscribe({
      next: (vacancies: Vacancy[]) => {
        this.allVacancies = vacancies;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading vacancies:', error);
        this.isLoading = false;
      }
    });
  }

  get filteredVacancies(): Vacancy[] {
    if (this.selectedCategory === 'ყველა') return this.allVacancies;
    return this.allVacancies.filter(v => v.category === this.selectedCategory);
  }

  getCount(category: string): number {
    if (category === 'ყველა') return this.allVacancies.length;
    return this.allVacancies.filter(v => v.category === category).length;
  }

  getCategoryClass(category: string): string {
    const map: { [key: string]: string } = {
      'ღია საჯარო კონკურსი': 'cat-open',
      'შიდა კონკურსი': 'cat-internal',
      'სააპლიკაციო ფორმა': 'cat-application'
    };
    return map[category] || 'cat-open';
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    if (isNaN(date.getTime())) return dateStr;
    const months = ['იან', 'თებ', 'მარ', 'აპრ', 'მაი', 'ივნ', 'ივლ', 'აგვ', 'სექ', 'ოქტ', 'ნოე', 'დეკ'];
    return `${date.getDate()} ${months[date.getMonth()]} ${date.getFullYear()}`;
  }
}
