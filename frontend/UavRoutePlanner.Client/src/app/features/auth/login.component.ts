import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { errorMessage } from '../../core/http/error-message';
import { AlertComponent } from '../../shared/ui/alert.component';

@Component({
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, AlertComponent],
  template: `
    <section class="card narrow">
      <h1>{{ registering() ? 'Create account' : 'Sign in' }}</h1>

      <form (ngSubmit)="submit()">
        <label>Username
          <input name="userName" [(ngModel)]="userName" autocomplete="username" required>
        </label>
        <label>Password
          <input name="password" type="password" [(ngModel)]="password"
                 [autocomplete]="registering() ? 'new-password' : 'current-password'" required>
        </label>

        <app-alert [message]="error()" />
        <app-alert [message]="notice()" kind="info" />

        <button class="primary" type="submit" [disabled]="busy()">
          {{ registering() ? 'Register' : 'Sign in' }}
        </button>
      </form>

      <button class="link" type="button" (click)="toggle()">
        {{ registering() ? 'Already have an account? Sign in' : 'No account yet? Register' }}
      </button>
    </section>
  `
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected userName = '';
  protected password = '';
  protected readonly registering = signal(false);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected toggle(): void {
    this.registering.update(value => !value);
    this.error.set(null);
    this.notice.set(null);
  }

  protected submit(): void {
    this.error.set(null);
    this.notice.set(null);
    this.busy.set(true);

    if (this.registering()) {
      this.auth.register(this.userName, this.password).subscribe({
        next: () => {
          this.busy.set(false);
          this.registering.set(false);
          this.notice.set('Account created. You can sign in now.');
        },
        error: (e: unknown) => this.fail(e)
      });
      return;
    }

    this.auth.login(this.userName, this.password).subscribe({
      next: () => void this.router.navigate(['/missions']),
      error: (e: unknown) => this.fail(e)
    });
  }

  private fail(error: unknown): void {
    this.busy.set(false);
    this.error.set(errorMessage(error));
  }
}
