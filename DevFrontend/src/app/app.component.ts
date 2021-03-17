import {Component} from '@angular/core';

@Component({
  selector: 'ph-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'DevFrontend';

  getYear(): Date {
    return new Date();
  }
}
