import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserService } from '../user.service';

const CIRCLE_GRADIENTS = [
  'linear-gradient(135deg, #0ea5e9 0%, #2563eb 55%, #4f46e5 100%)',
  'linear-gradient(135deg, #f97316 0%, #f59e0b 55%, #22c55e 100%)',
  'linear-gradient(135deg, #22c55e 0%, #06b6d4 55%, #3b82f6 100%)'
];

const FEATURE_GRADIENTS = [
  'linear-gradient(135deg, #0f172a 0%, #1d4ed8 60%, #22c55e 100%)',
  'linear-gradient(135deg, #1e1b4b 0%, #7c3aed 60%, #06b6d4 100%)',
  'linear-gradient(135deg, #0c4a6e 0%, #0284c7 60%, #34d399 100%)',
  'linear-gradient(135deg, #14532d 0%, #16a34a 60%, #fbbf24 100%)'
];

@Component({
  selector: 'app-digest',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './digest.component.html',
  styleUrl: './digest.component.css'
})
export class DigestComponent implements OnInit {
  featuredEntries: any[] = [];
  listEntries: any[] = [];
  periodText = '';

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.userService.getDigestEntries(true).subscribe({
      next: (entries) => {
        this.featuredEntries = entries.filter(e => e.isFeatured).slice(0, 3);
        this.listEntries = entries.filter(e => !e.isFeatured);
        this.buildPeriodText(entries);
      },
      error: () => {
        this.periodText = '';
      }
    });
  }

  private buildPeriodText(entries: any[]): void {
    if (!entries.length) { this.periodText = ''; return; }
    const dates = entries.flatMap(e => [new Date(e.periodFrom), new Date(e.periodTo)]);
    const min = new Date(Math.min(...dates.map(d => d.getTime())));
    const max = new Date(Math.max(...dates.map(d => d.getTime())));
    const months = ['იანვარი','თებერვალი','მარტი','აპრილი','მაისი','ივნისი',
                    'ივლისი','აგვისტო','სექტემბერი','ოქტომბერი','ნოემბერი','დეკემბერი'];
    if (min.getMonth() === max.getMonth() && min.getFullYear() === max.getFullYear()) {
      this.periodText = `${min.getDate()} - ${max.getDate()} ${months[min.getMonth()]}, ${min.getFullYear()}`;
    } else {
      this.periodText = `${min.getDate()} ${months[min.getMonth()]} - ${max.getDate()} ${months[max.getMonth()]}, ${max.getFullYear()}`;
    }
  }

  getCircleBackground(index: number, entry: any): string {
    if (entry.imageUrl) return `url(${entry.imageUrl}) center/cover no-repeat`;
    return CIRCLE_GRADIENTS[index % CIRCLE_GRADIENTS.length];
  }

  getFeatureBackground(index: number, entry: any): string {
    if (entry.imageUrl) return `url(${entry.imageUrl}) center/cover no-repeat`;
    return FEATURE_GRADIENTS[index % FEATURE_GRADIENTS.length];
  }
}
