import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  QueryList,
  ViewChild,
  ViewChildren
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService } from '../user.service';

interface NewsItem {
  newsID: number;
  title: string;
  content: string;
  imageUrl?: string;
  authorName: string;
  createdAt: string;
}

@Component({
  selector: 'app-news',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './news.component.html',
  styleUrl: './news.component.css'
})
export class NewsComponent implements OnInit, AfterViewInit, OnDestroy {
  news: NewsItem[] = [];
  loading = true;
  selectedNews: NewsItem | null = null;
  modalOpen = false;

  @ViewChild('newsGrid') private newsGridRef?: ElementRef<HTMLDivElement>;
  @ViewChildren('newsCard') private newsCardRefs?: QueryList<ElementRef<HTMLElement>>;

  private resizeObserver?: ResizeObserver;
  private cardResizeObservers: ResizeObserver[] = [];
  private layoutRafId: number | null = null;

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.loadNews();
  }

  ngAfterViewInit(): void {
    // Re-layout when container width changes (responsive / sidebar / etc.)
    if (typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver(() => this.scheduleLayout());
      if (this.newsGridRef?.nativeElement) {
        this.resizeObserver.observe(this.newsGridRef.nativeElement);
      }
    }

    // Re-layout when list of cards changes
    this.newsCardRefs?.changes.subscribe(() => {
      this.attachCardObservers();
      this.scheduleLayout();
    });
  }

  ngOnDestroy(): void {
    if (this.layoutRafId !== null) {
      cancelAnimationFrame(this.layoutRafId);
      this.layoutRafId = null;
    }

    this.resizeObserver?.disconnect();
    this.resizeObserver = undefined;

    for (const obs of this.cardResizeObservers) obs.disconnect();
    this.cardResizeObservers = [];
  }

  loadNews(): void {
    this.userService.getNews().subscribe({
      next: (news) => {
        this.news = [...news].sort((a, b) => {
          const dateA = new Date(a.createdAt).getTime();
          const dateB = new Date(b.createdAt).getTime();
          return dateB - dateA;
        });
        this.loading = false;
        // Wait for DOM paint, then layout
        this.attachCardObservers();
        this.scheduleLayout();
      },
      error: (error) => {
        console.error('Error loading news:', error);
        this.loading = false;
      }
    });
  }

  scheduleLayout(): void {
    if (this.layoutRafId !== null) return;
    this.layoutRafId = requestAnimationFrame(() => {
      this.layoutRafId = null;
      this.layoutMasonry();
    });
  }

  private attachCardObservers(): void {
    // Card heights can change after images load, fonts settle, etc.
    for (const obs of this.cardResizeObservers) obs.disconnect();
    this.cardResizeObservers = [];

    if (typeof ResizeObserver === 'undefined') return;
    const cards = this.newsCardRefs?.toArray().map((r) => r.nativeElement) ?? [];
    for (const card of cards) {
      const obs = new ResizeObserver(() => this.scheduleLayout());
      obs.observe(card);
      this.cardResizeObservers.push(obs);
    }
  }

  private layoutMasonry(): void {
    const grid = this.newsGridRef?.nativeElement;
    const cards = this.newsCardRefs?.toArray().map((r) => r.nativeElement) ?? [];
    if (!grid || cards.length === 0) return;

    // Match the UI gaps you wanted
    const gap = window.matchMedia('(max-width: 768px)').matches ? 16 : 24;

    // Card width comes from CSS; read it from the first card
    const cardWidth = Math.round(cards[0].getBoundingClientRect().width);
    const containerWidth = Math.round(grid.getBoundingClientRect().width);
    if (cardWidth <= 0 || containerWidth <= 0) return;

    const columns = Math.max(1, Math.floor((containerWidth + gap) / (cardWidth + gap)));
    const totalColumnsWidth = columns * cardWidth + (columns - 1) * gap;
    const leftOffset = Math.max(0, Math.floor((containerWidth - totalColumnsWidth) / 2));

    const columnHeights = new Array<number>(columns).fill(0);

    // Important: place items in array order (newest first), left->right for first row,
    // then continue packing into the shortest column (Pinterest-like).
    for (let i = 0; i < cards.length; i++) {
      const card = cards[i];

      const colIndex =
        i < columns
          ? i // first row: strictly left->right
          : columnHeights.indexOf(Math.min(...columnHeights)); // then: shortest column

      const x = leftOffset + colIndex * (cardWidth + gap);
      const y = columnHeights[colIndex];

      card.style.position = 'absolute';
      card.style.left = '0px';
      card.style.top = '0px';
      card.style.transform = `translate(${x}px, ${y}px)`;

      const cardHeight = Math.round(card.getBoundingClientRect().height);
      columnHeights[colIndex] = y + cardHeight + gap;
    }

    const height = Math.max(...columnHeights);
    grid.style.height = `${Math.max(0, height - gap)}px`;
  }

  hasImage(newsItem: NewsItem): boolean {
    return !!newsItem.imageUrl && newsItem.imageUrl.trim() !== '';
  }

  isLongContent(content: string): boolean {
    return content.length > 200;
  }

  getTruncatedContent(content: string): string {
    if (content.length <= 200) {
      return content;
    }
    return content.substring(0, 200) + '...';
  }

  openNewsModal(newsItem: NewsItem): void {
    this.selectedNews = newsItem;
    this.modalOpen = true;
    document.body.style.overflow = 'hidden';
  }

  closeModal(): void {
    this.modalOpen = false;
    this.selectedNews = null;
    document.body.style.overflow = 'auto';
  }
}
