import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactPick } from '../../personalize.types';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-personalize-send-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="
        position:fixed; inset:0; z-index:9999;
        background:rgba(20,16,8,0.55);
        backdrop-filter:blur(8px);
        display:flex; align-items:center; justify-content:center;
        padding:20px;
        animation:fadeInBackdrop 0.2s ease;
      " (click)="close.emit()">

      <div (click)="$event.stopPropagation()" style="
          background:#FFFDF7;
          border-radius:24px;
          width:100%; max-width:500px;
          max-height:90vh;
          overflow-y:auto;
          box-shadow:0 24px 80px rgba(0,0,0,0.25);
          animation:slideUpModal 0.25s cubic-bezier(0.34,1.56,0.64,1);
        ">

        <div style="padding:24px 28px 0; display:flex; justify-content:space-between; align-items:center;">
          <div>
            <h2 style="font-family:'Playfair Display',serif;font-size:22px;font-weight:700;color:#1C1A16;margin:0 0 4px;">Send this Card</h2>
            <p style="font-size:13px;color:#8B7860;margin:0;">Choose a recipient and add a subject to send</p>
          </div>
          <button (click)="close.emit()" style="
              width:36px;height:36px;border-radius:50%;border:none;
              background:#F5EDD8;color:#6B6560;cursor:pointer;
              display:flex;align-items:center;justify-content:center;
              font-size:20px;flex-shrink:0;">
            <span class="material-symbols-outlined" style="font-size:20px;">close</span>
          </button>
        </div>

        <div style="padding:20px 28px 28px;">
          @if (contacts().length > 0) {
            <div style="margin-bottom:20px;">
              <label style="font-size:12px;font-weight:700;color:#8B6914;display:block;margin-bottom:10px;">SELECT FROM CONTACTS</label>
              <div style="display:flex;flex-direction:column;gap:6px;max-height:200px;overflow-y:auto;
                          border:1px solid #EDE6D6;border-radius:12px;padding:6px;">
                @for (c of contacts(); track c.id) {
                  <button (click)="selectContact(c)"
                    style="display:flex;align-items:center;gap:12px;padding:10px 12px;
                           border-radius:10px;border:none;cursor:pointer;text-align:left;
                           transition:background 0.15s;"
                    [style.background]="selectedContact()?.id===c.id ? '#F5EDD8' : '#fff'"
                    [style.outline]="selectedContact()?.id===c.id ? '2px solid #8B6914' : 'none'">
                    <div style="width:34px;height:34px;border-radius:50%;background:#F5EDD8;color:#8B6914;
                                font-weight:700;font-size:13px;display:flex;align-items:center;justify-content:center;flex-shrink:0;">
                      {{ c.name.charAt(0).toUpperCase() }}
                    </div>
                    <div style="min-width:0;">
                      <div style="font-size:13px;font-weight:600;color:#1C1A16;">{{ c.name }}</div>
                      <div style="font-size:12px;color:#8B7860;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;">{{ c.email }}</div>
                    </div>
                    @if (selectedContact()?.id===c.id) {
                      <span class="material-symbols-outlined" style="color:#8B6914;font-size:18px;margin-left:auto;flex-shrink:0;">check_circle</span>
                    }
                  </button>
                }
              </div>
            </div>

            <div style="display:flex;align-items:center;gap:10px;margin-bottom:20px;">
              <div style="flex:1;height:1px;background:#EDE6D6;"></div>
              <span style="font-size:12px;color:#B8A88A;font-weight:600;">OR</span>
              <div style="flex:1;height:1px;background:#EDE6D6;"></div>
            </div>
          }

          <div style="margin-bottom:14px;">
            <label style="font-size:12px;font-weight:700;color:#8B6914;display:block;margin-bottom:6px;">RECIPIENT EMAIL *</label>
            <div style="position:relative;">
              <span class="material-symbols-outlined" style="position:absolute;left:12px;top:50%;transform:translateY(-50%);font-size:18px;color:#C9A96E;pointer-events:none;">alternate_email</span>
              <input type="email" [ngModel]="recipientEmail()" (ngModelChange)="recipientEmail.set($event)"
                placeholder="friend@example.com"
                style="width:100%;padding:12px 12px 12px 40px;border:1.5px solid #E0D5C0;border-radius:12px;
                       font-size:14px;outline:none;box-sizing:border-box;transition:border-color 0.2s;background:#FFFDF7;"
                [style.border-color]="recipientEmail() && !isValidEmail(recipientEmail()) ? '#dc2626' : (recipientEmail() ? '#8B6914' : '#E0D5C0')">
            </div>
            @if (recipientEmail() && !isValidEmail(recipientEmail())) {
              <span style="font-size:11px;color:#dc2626;margin-top:3px;display:block;">Invalid email address</span>
            }
          </div>

          <div style="margin-bottom:20px;">
            <label style="font-size:12px;font-weight:700;color:#8B6914;display:block;margin-bottom:6px;">SUBJECT *</label>
            <div style="position:relative;">
              <span class="material-symbols-outlined" style="position:absolute;left:12px;top:50%;transform:translateY(-50%);font-size:18px;color:#C9A96E;pointer-events:none;">subject</span>
              <input type="text" [ngModel]="subject()" (ngModelChange)="subject.set($event)"
                placeholder="A warm greeting for you!"
                style="width:100%;padding:12px 12px 12px 40px;border:1.5px solid #E0D5C0;border-radius:12px;
                       font-size:14px;outline:none;box-sizing:border-box;transition:border-color 0.2s;background:#FFFDF7;"
                [style.border-color]="subject() ? '#8B6914' : '#E0D5C0'">
            </div>
          </div>

          @if (sendError()) {
            <div style="padding:12px;background:#FEE2E2;border-radius:10px;font-size:13px;color:#991B1B;margin-bottom:14px;">
              {{ sendError() }}
            </div>
          }

          <div style="display:flex;gap:10px;">
            <button (click)="close.emit()"
              style="flex:1;padding:14px;border-radius:12px;border:1.5px solid #E0D5C0;background:#fff;color:#6B6560;
                     font-size:14px;font-weight:600;cursor:pointer;transition:all 0.15s;">
              Cancel
            </button>
            <button (click)="confirm.emit()" [disabled]="sending()"
              style="flex:2;padding:14px;border-radius:12px;border:none;
                     background:linear-gradient(135deg,#A07830,#8B6914);color:#fff;font-size:14px;font-weight:700;
                     cursor:pointer;display:flex;align-items:center;justify-content:center;gap:8px;
                     box-shadow:0 4px 16px rgba(139,105,20,0.3);transition:opacity 0.15s;"
              [style.opacity]="sending() ? '0.7' : '1'">
              @if (sending()) {
                <span class="material-symbols-outlined" style="font-size:18px;animation:spin 1s linear infinite;">progress_activity</span>
                Sending...
              } @else {
                <span class="material-symbols-outlined" style="font-size:18px;">{{ scheduleEnabled() ? 'schedule_send' : 'send' }}</span>
                {{ scheduleEnabled() ? 'Schedule Send' : 'Send Now' }}
              }
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PersonalizeSendModalComponent {
  contacts = input<ContactPick[]>([]);
  selectedContact = model<ContactPick | null>(null);
  recipientEmail = model<string>('');
  subject = model<string>('');
  sending = input<boolean>(false);
  sendError = input<string>('');
  scheduleEnabled = input<boolean>(false);

  close = output<void>();
  confirm = output<void>();

  selectContact(c: ContactPick) {
    this.selectedContact.set(c);
    this.recipientEmail.set(c.email);
  }

  isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }
}
