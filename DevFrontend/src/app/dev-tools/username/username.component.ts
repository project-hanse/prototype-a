import {Component, OnInit} from '@angular/core';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {UsersService} from '../_services/users.service';

@Component({
	selector: 'ph-username',
	templateUrl: './username.component.html',
	styleUrls: ['./username.component.scss']
})
export class UsernameComponent implements OnInit {
	$userInfo: Observable<{ username: string }>;

	constructor(private usersService: UsersService) {
		this.$userInfo = this.usersService.getCurrentUserInfo().pipe(map(info => {
			if (!info) {
				return info;
			}
			info.username = info?.username?.trim() || 'Anonymous';
			return info;
		}));
	}

	ngOnInit(): void {
	}

}
