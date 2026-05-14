import { Component, OnDestroy, OnInit, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NavbarComponent } from '../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../shared/components/footer/footer.component';
import { ColorWheelComponent } from './components/color-wheel/color-wheel.component';
import { PersonalizeCanvasComponent } from './components/personalize-canvas/personalize-canvas.component';
import { PersonalizeTextEditorComponent } from './components/personalize-text-editor/personalize-text-editor.component';
import { PersonalizeDesignPanelComponent } from './components/personalize-design-panel/personalize-design-panel.component';
import { PersonalizeTypographyPanelComponent } from './components/personalize-typography-panel/personalize-typography-panel.component';
import { PersonalizeSendModalComponent } from './components/personalize-send-modal/personalize-send-modal.component';
import { PersonalizeStateService } from './services/personalize-state.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-personalize',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink,
    NavbarComponent, FooterComponent, ColorWheelComponent,
    PersonalizeCanvasComponent, PersonalizeTextEditorComponent,
    PersonalizeDesignPanelComponent, PersonalizeTypographyPanelComponent,
    PersonalizeSendModalComponent
  ],
  providers: [PersonalizeStateService],
  styleUrl: './personalize.component.css',
  templateUrl: './personalize.component.html'
})
export class PersonalizeComponent implements OnInit, OnDestroy {
  protected state = inject(PersonalizeStateService);

  ngOnInit(): void { this.state.init(); }
  ngOnDestroy(): void { this.state.dispose(); }
}
