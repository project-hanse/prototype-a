import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DevToolsRoutingModule } from './dev-tools-routing.module';
import { StatusComponent } from './status/status.component';
import { StatusBarComponent } from './status/status-bar/status-bar.component';
import {CoreModule} from '../core/core.module';


@NgModule({
    declarations: [
        StatusComponent,
        StatusBarComponent
    ],
    exports: [
        StatusBarComponent
    ],
    imports: [
        CommonModule,
        DevToolsRoutingModule,
        CoreModule
    ]
})
export class DevToolsModule { }
