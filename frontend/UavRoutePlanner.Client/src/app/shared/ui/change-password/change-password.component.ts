import { ChangeDetectionStrategy, Component, ElementRef, afterNextRender, inject, output, signal, viewChild } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../core/auth/auth.service';
import { errorMessage } from '../../../core/http/error-message';

const MIN_PASSWORD_LENGTH = 6;

const passwordsMatch = (group: AbstractControl): ValidationErrors | null =>
  group.get('next')?.value === group.get('confirm')?.value ? null : { mismatch: true };

@Component({
  selector: 'app-change-password',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css',
  host: { '(document:keydown.escape)': 'close()' }
})
export class ChangePasswordComponent {
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly firstField = viewChild<ElementRef<HTMLInputElement>>('firstField');

  readonly closed = output<void>();

  protected readonly form = this.fb.nonNullable.group(
    {
      current: ['', [Validators.required]],
      next: ['', [Validators.required, Validators.minLength(MIN_PASSWORD_LENGTH)]],
      confirm: ['', [Validators.required]]
    },
    { validators: [passwordsMatch] }
  );

  protected readonly minLength = MIN_PASSWORD_LENGTH;
  protected readonly busy = signal(false);
  protected readonly done = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly submitted = signal(false);
  protected readonly visible = signal(false);

  constructor() {
    afterNextRender(() => this.firstField()?.nativeElement.focus());
  }

  protected close(): void {
    if (!this.busy()) {
      this.closed.emit();
    }
  }

  protected toggleVisible(): void {
    this.visible.update(value => !value);
  }

  protected invalid(name: 'current' | 'next' | 'confirm'): boolean {
    const control = this.form.controls[name];
    return control.invalid && (control.touched || this.submitted());
  }

  protected mismatch(): boolean {
    return this.form.hasError('mismatch') && (this.form.controls.confirm.touched || this.submitted());
  }

  protected submit(): void {
    this.submitted.set(true);
    this.error.set(null);
    this.form.markAllAsTouched();
    if (this.form.invalid || this.busy()) {
      return;
    }

    const { current, next } = this.form.getRawValue();
    this.busy.set(true);
    this.auth.changePassword(current, next).subscribe({
      next: () => {
        this.busy.set(false);
        this.done.set(true);
      },
      error: (e: unknown) => {
        this.busy.set(false);
        this.error.set(errorMessage(e));
      }
    });
  }
}
