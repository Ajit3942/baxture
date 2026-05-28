import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

type ExportFormat = 'PDF' | 'EXCEL';

interface UserRecord {
  id: string;
  username: string;
  isAdmin: boolean;
  age: number;
  hobbies: string[];
}

interface LoginResponse {
  token: string;
  user: UserRecord;
}

interface SearchResponse {
  items: UserRecord[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

interface UserForm {
  username: string;
  password: string;
  age: number | null;
  hobbiesText: string;
  isAdmin: boolean;
}

@Component({
  selector: 'app-root',
  imports: [FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly apiBaseUrl = 'https://localhost:7077/api';

  protected readonly token = signal(localStorage.getItem('baxture.token') ?? '');
  protected readonly currentUser = signal<UserRecord | null>(null);
  protected readonly users = signal<UserRecord[]>([]);
  protected readonly totalCount = signal(0);
  protected readonly totalPages = signal(0);
  protected readonly selectedUserId = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly loading = signal(false);

  protected loginForm = {
    username: 'admin',
    password: 'admin123'
  };

  protected userForm: UserForm = this.emptyUserForm();

  protected searchForm = {
    fieldName: 'username',
    fieldValue: '',
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'username',
    sortDirection: 'asc'
  };

  protected readonly isLoggedIn = computed(() => this.token().length > 0);
  protected readonly isEditing = computed(() => this.selectedUserId() !== null);

  constructor(private readonly http: HttpClient) {
    if (this.token()) {
      this.loadUsers();
    }
  }

  protected login(): void {
    this.startRequest();
    this.http.post<LoginResponse>(`${this.apiBaseUrl}/auth/login`, this.loginForm).subscribe({
      next: (response) => {
        localStorage.setItem('baxture.token', response.token);
        this.token.set(response.token);
        this.currentUser.set(response.user);
        this.message.set(`Signed in as ${response.user.username}.`);
        this.loadUsers();
      },
      error: (error) => this.handleError(error)
    });
  }

  protected logout(): void {
    localStorage.removeItem('baxture.token');
    this.token.set('');
    this.currentUser.set(null);
    this.users.set([]);
    this.totalCount.set(0);
    this.totalPages.set(0);
    this.selectedUserId.set(null);
    this.message.set('Signed out.');
    this.error.set('');
  }

  protected loadUsers(): void {
    this.startRequest();
    this.http.get<UserRecord[]>(`${this.apiBaseUrl}/users`, { headers: this.authHeaders() }).subscribe({
      next: (users) => {
        this.users.set(users);
        this.totalCount.set(users.length);
        this.totalPages.set(users.length > 0 ? 1 : 0);
        this.message.set('Users loaded.');
        this.loading.set(false);
      },
      error: (error) => this.handleError(error)
    });
  }

  protected searchUsers(): void {
    const filters = this.searchForm.fieldValue.trim()
      ? [{ fieldName: this.searchForm.fieldName, fieldValue: this.searchForm.fieldValue.trim() }]
      : [];

    this.startRequest();
    this.http.post<SearchResponse>(`${this.apiBaseUrl}/users/search`, {
      filters,
      pageNumber: this.searchForm.pageNumber,
      pageSize: this.searchForm.pageSize,
      sortBy: this.searchForm.sortBy,
      sortDirection: this.searchForm.sortDirection
    }, { headers: this.authHeaders() }).subscribe({
      next: (response) => {
        this.users.set(response.items);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
        this.message.set('Search complete.');
        this.loading.set(false);
      },
      error: (error) => this.handleError(error)
    });
  }

  protected saveUser(): void {
    const body = {
      username: this.userForm.username.trim(),
      password: this.userForm.password.trim(),
      isAdmin: this.userForm.isAdmin,
      age: this.userForm.age,
      hobbies: this.parseHobbies(this.userForm.hobbiesText)
    };

    if (this.selectedUserId()) {
      this.updateUser(body);
      return;
    }

    this.startRequest();
    this.http.post<UserRecord>(`${this.apiBaseUrl}/users`, body, { headers: this.authHeaders() }).subscribe({
      next: (user) => {
        this.message.set(`Created ${user.username}.`);
        this.resetForm();
        this.loadUsers();
      },
      error: (error) => this.handleError(error)
    });
  }

  protected editUser(user: UserRecord): void {
    this.selectedUserId.set(user.id);
    this.userForm = {
      username: user.username,
      password: '',
      age: user.age,
      hobbiesText: user.hobbies.join(', '),
      isAdmin: user.isAdmin
    };
    this.message.set(`Editing ${user.username}. Leave password empty to keep the current password.`);
  }

  protected resetForm(): void {
    this.selectedUserId.set(null);
    this.userForm = this.emptyUserForm();
  }

  protected deleteUser(user: UserRecord): void {
    if (!confirm(`Delete ${user.username}?`)) {
      return;
    }

    this.startRequest();
    this.http.delete(`${this.apiBaseUrl}/users/${user.id}`, { headers: this.authHeaders() }).subscribe({
      next: () => {
        this.message.set(`Deleted ${user.username}.`);
        this.loadUsers();
      },
      error: (error) => this.handleError(error)
    });
  }

  protected exportUsers(format: ExportFormat): void {
    const filters = this.searchForm.fieldValue.trim()
      ? [{ fieldName: this.searchForm.fieldName, fieldValue: this.searchForm.fieldValue.trim() }]
      : [];

    this.startRequest();
    this.http.post(`${this.apiBaseUrl}/users/export`, {
      format,
      search: {
        filters,
        pageNumber: this.searchForm.pageNumber,
        pageSize: this.searchForm.pageSize,
        sortBy: this.searchForm.sortBy,
        sortDirection: this.searchForm.sortDirection
      }
    }, {
      headers: this.authHeaders(),
      responseType: 'blob',
      observe: 'response'
    }).subscribe({
      next: (response) => {
        const blob = response.body;
        if (!blob) {
          this.handleErrorMessage('Export returned no file content.');
          return;
        }

        const extension = format === 'PDF' ? 'pdf' : 'csv';
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = `users-export.${extension}`;
        anchor.click();
        URL.revokeObjectURL(url);
        this.message.set(`${format} export downloaded.`);
        this.loading.set(false);
      },
      error: (error) => this.handleError(error)
    });
  }

  private updateUser(body: { username: string; password: string; isAdmin: boolean; age: number | null; hobbies: string[] }): void {
    const userId = this.selectedUserId();
    if (!userId) {
      return;
    }

    const updateBody: Record<string, unknown> = {
      username: body.username,
      isAdmin: body.isAdmin,
      age: body.age,
      hobbies: body.hobbies
    };

    if (body.password) {
      updateBody['password'] = body.password;
    }

    this.startRequest();
    this.http.put<UserRecord>(`${this.apiBaseUrl}/users/${userId}`, updateBody, { headers: this.authHeaders() }).subscribe({
      next: (user) => {
        this.message.set(`Updated ${user.username}.`);
        this.resetForm();
        this.loadUsers();
      },
      error: (error) => this.handleError(error)
    });
  }

  private emptyUserForm(): UserForm {
    return {
      username: '',
      password: '',
      age: null,
      hobbiesText: '',
      isAdmin: false
    };
  }

  private parseHobbies(value: string): string[] {
    return value
      .split(',')
      .map((hobby) => hobby.trim())
      .filter(Boolean);
  }

  private authHeaders(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.token()}` });
  }

  private startRequest(): void {
    this.loading.set(true);
    this.error.set('');
    this.message.set('');
  }

  private handleError(error: HttpErrorResponse): void {
    const apiMessage = error.error?.message;
    const validationErrors = Array.isArray(error.error?.errors) ? ` ${error.error.errors.join(' ')}` : '';
    this.handleErrorMessage(`${apiMessage || error.message || 'Request failed.'}${validationErrors}`);
  }

  private handleErrorMessage(message: string): void {
    this.error.set(message);
    this.loading.set(false);
  }
}
