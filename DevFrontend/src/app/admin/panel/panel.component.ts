import {Component, OnInit} from '@angular/core';

@Component({
	selector: 'ph-panel',
	templateUrl: './panel.component.html',
	styleUrls: ['./panel.component.scss']
})
export class PanelComponent implements OnInit {
	links: Array<{ title: string, path: string, icon: string }> = [
		{
			title: 'Models',
			path: '../model',
			icon: 'smart_toy'
		}
	];

	constructor() {
	}

	ngOnInit(): void {
	}

}
