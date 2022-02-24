import {Component, Input, OnInit} from '@angular/core';
import {PipelineService} from '../../_service/pipeline.service';

@Component({
	selector: 'ph-pipeline-export',
	templateUrl: './pipeline-export.component.html',
	styleUrls: ['./pipeline-export.component.scss']
})
export class PipelineExportComponent implements OnInit {

	@Input() pipelineId?: string;
	@Input() compact: boolean = false;

	constructor(private pipelineService: PipelineService) {
	}

	ngOnInit(): void {
	}

	getDownloadLink(id: string): string {
		return this.pipelineService.getPipelineDownloadLink(id);
	}

}
