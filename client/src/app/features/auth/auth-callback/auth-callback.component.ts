import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Stub route for future OAuth / external identity provider callbacks. */
@Component({
  selector: 'app-auth-callback',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="card callback">
      <h2>Authentication callback</h2>
      <p>
        External sign-in is not wired yet. This route is reserved for OAuth redirects in a later
        phase.
      </p>
      <a routerLink="/login" class="btn btn--primary">Back to sign in</a>
    </div>
  `,
  styles: [
    `
      .callback {
        max-width: 480px;
        margin: 4rem auto;
        text-align: center;

        h2 {
          font-family: var(--font-display);
        }

        p {
          color: var(--text-muted);
          line-height: 1.6;
        }
      }
    `
  ]
})
export class AuthCallbackComponent {}
