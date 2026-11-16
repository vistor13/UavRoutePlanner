import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/auth/auth.service';
import { errorMessage } from '../../core/http/error-message';
import { LogoComponent } from '../../shared/ui/logo.component';

type Mode = 'login' | 'register';

const passwordsMatch = (group: AbstractControl): ValidationErrors | null => {
  const password = group.get('password')?.value;
  const confirm = group.get('confirm')?.value;
  return password === confirm ? null : { mismatch: true };
};

@Component({
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, LogoComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  protected readonly form = this.fb.nonNullable.group(
    {
      userName: ['', [Validators.required]],
      password: ['', [Validators.required]],
      confirm: ['']
    },
    { validators: [] }
  );

  protected readonly mode = signal<Mode>('login');
  protected readonly busy = signal(false);
  protected readonly showPassword = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly submitted = signal(false);

  protected readonly registering = computed(() => this.mode() === 'register');
  protected readonly throttled = computed(() => (this.error() ?? '').toLowerCase().startsWith('too many'));

  private readonly passwordValue = toSignal(this.form.controls.password.valueChanges, { initialValue: '' });

  protected readonly strength = computed(() => {
    const value = this.passwordValue();
    if (!value) {
      return { level: 0, label: '' };
    }
    let score = 0;
    if (value.length >= 8) score++;
    if (/[a-z]/.test(value) && /[A-Z]/.test(value)) score++;
    if (/\d/.test(value)) score++;
    if (/[^A-Za-z0-9]/.test(value)) score++;
    if (value.length < 6) score = Math.min(score, 1);
    const labels = ['Weak', 'Weak', 'Fair', 'Good', 'Strong'];
    return { level: Math.max(score, 1), label: labels[Math.max(score, 1)] };
  });

  protected setMode(mode: Mode): void {
    if (this.mode() === mode) {
      return;
    }
    this.mode.set(mode);
    this.error.set(null);
    this.notice.set(null);
    this.submitted.set(false);
    this.showPassword.set(false);
    this.form.controls.password.reset('');
    this.form.controls.confirm.reset('');
    this.form.setValidators(mode === 'register' ? [passwordsMatch] : []);
    this.form.controls.confirm.setValidators(mode === 'register' ? [Validators.required] : []);
    this.form.controls.confirm.updateValueAndValidity();
    this.form.updateValueAndValidity();
  }

  protected togglePassword(): void {
    this.showPassword.update(value => !value);
  }

  protected invalid(name: 'userName' | 'password' | 'confirm'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected mismatch(): boolean {
    return this.registering() && this.form.hasError('mismatch') && (this.form.controls.confirm.touched || this.submitted());
  }

  protected submit(): void {
    this.submitted.set(true);
    this.error.set(null);
    this.notice.set(null);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.busy()) {
      return;
    }

    const { userName, password } = this.form.getRawValue();
    this.busy.set(true);

    if (this.registering()) {
      this.auth.register(userName, password).subscribe({
        next: () => {
          this.busy.set(false);
          this.setMode('login');
          this.notice.set('Account created. You can sign in now.');
        },
        error: (e: unknown) => this.fail(e)
      });
      return;
    }

    this.auth.login(userName, password).subscribe({
      next: () => void this.router.navigate(['/missions']),
      error: (e: unknown) => this.fail(e)
    });
  }

  private fail(error: unknown): void {
    this.busy.set(false);
    this.error.set(errorMessage(error));
  }
}
