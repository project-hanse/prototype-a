import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {environment} from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BaseHttpService {

  constructor(private basePath: string, private http: HttpClient) {
  }

  public get<T>(path: string = ''): Observable<T> {
    return this.http.get<T>(environment.apiUrl + '/' + this.basePath + '/' + path);
  }
}
