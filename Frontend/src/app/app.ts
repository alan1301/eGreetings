import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthModalComponent } from './shared/components/auth-modal/auth-modal.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthModalComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {}
