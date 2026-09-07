import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateUserRequest, EntityStatus, UpdateUserRequest, User } from './user.models';

@Injectable({ providedIn: 'root' })
export class UsersApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/v1/users';

  list(status?: EntityStatus): Observable<readonly User[]> {
    const options =
      status === undefined
        ? undefined
        : { params: new HttpParams().set('status', status === 1 ? 'Active' : 'Inactive') };
    return this.http.get<readonly User[]>(this.baseUrl, options);
  }

  create(request: CreateUserRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request);
  }

  update(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, request);
  }
}
