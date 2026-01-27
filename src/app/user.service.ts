import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';

export interface CurrentUser {
  name: string;
  initials: string;
  title: string;
  role: string;
  userId: number;
}

export interface User {
  userID: number;
  name: string;
  role: string;
  department: string;
  email: string;
  title: string;
}

export interface CreateUserDto {
  name: string;
  email: string;
  role: string;
  department: string;
  title: string;
}

export interface UpdateUserDto {
  name: string;
  email: string;
  role: string;
  department: string;
  title: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5250/api';

  private currentUserSubject = new BehaviorSubject<CurrentUser>({
    name: 'გიორგი მაისურაძე',
    initials: 'გმ',
    title: 'პროექტის მენეჯერი',
    role: 'შემვსები',
    userId: 1
  });

  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  private users: { [key: string]: CurrentUser } = {
    'შემვსები': {
      name: 'გიორგი მაისურაძე',
      initials: 'გმ',
      title: 'პროექტის მენეჯერი',
      role: 'შემვსები',
      userId: 1
    },
    'ხელმძღვანელი': {
      name: 'ნინო ბერიძე',
      initials: 'ნბ',
      title: 'IT დეპარტამენტის ხელმძღვანელი',
      role: 'ხელმძღვანელი',
      userId: 2
    },
    'შემსრულებელი': {
      name: 'დავით კვარაცხელია',
      initials: 'დკ',
      title: 'ტექნიკური სპეციალისტი',
      role: 'შემსრულებელი',
      userId: 3
    },
    'ადმინისტრატორი': {
      name: 'ანა გელაშვილი',
      initials: 'აგ',
      title: 'სისტემის ადმინისტრატორი',
      role: 'ადმინისტრატორი',
      userId: 4
    }
  };

  setUserRole(role: string) {
    const user = this.users[role] || this.users['შემვსები'];
    this.currentUserSubject.next(user);
  }

  setUserByRole(role: string) {
    const user = this.users[role] || this.users['შემვსები'];
    this.currentUserSubject.next(user);
  }

  getCurrentUser(): CurrentUser {
    return this.currentUserSubject.value;
  }

  // User management methods
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/users/${id}`);
  }

  createUser(user: CreateUserDto): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/users`, user);
  }

  updateUser(id: number, user: UpdateUserDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${id}`, user);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${id}`);
  }

  // News management methods
  getNews(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/news`);
  }

  getNewsItem(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/news/${id}`);
  }

  createNews(newsData: any): Observable<any> {
    // Pass the news data as-is, authorID should be included by the component
    return this.http.post<any>(`${this.apiUrl}/news`, newsData);
  }

  updateNews(id: number, newsData: any): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/news/${id}`, newsData);
  }

  deleteNews(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/news/${id}`);
  }
}
