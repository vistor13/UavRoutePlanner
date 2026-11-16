import { ChangeDetectionStrategy, Component, ElementRef, OnInit, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../../core/auth/auth.service';
import { ChangePasswordComponent } from '../change-password/change-password.component';

@Component({
  selector: 'app-user-menu',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ChangePasswordComponent],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.css',
  host: {
    '(document:click)': 'onDocumentClick($event)',
    '(document:keydown.escape)': 'close()'
  }
})
export class UserMenuComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected readonly userName = this.auth.userName;
  protected readonly open = signal(false);
  protected readonly changing = signal(false);

  protected readonly initials = computed(() => {
    const name = this.userName();
    if (!name) {
      return '?';
    }
    const words = name.split(/[^\p{L}\p{N}]+/u).filter(Boolean);
    const letters = words.length > 1 ? words[0][0] + words[1][0] : name.slice(0, 2);
    return letters.toUpperCase();
  });

  ngOnInit(): void {
    if (this.userName() === null) {
      this.auth.loadProfile().subscribe({ error: () => undefined });
    }
  }

  protected toggle(): void {
    this.open.update(value => !value);
  }

  protected close(): void {
    this.open.set(false);
  }

  protected openChangePassword(): void {
    this.open.set(false);
    this.changing.set(true);
  }

  protected signOut(): void {
    this.open.set(false);
    this.auth.logout();
  }

  protected onDocumentClick(event: MouseEvent): void {
    if (this.open() && !this.host.nativeElement.contains(event.target as Node)) {
      this.open.set(false);
    }
  }
}
