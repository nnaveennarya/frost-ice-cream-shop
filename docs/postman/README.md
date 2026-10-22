# Frost Postman collection

Import `Frost.postman_collection.json` and `Frost.local.postman_environment.json` into Postman, then select the Frost environment.

## Connect and sign in

1. Start MySQL and the Frost backend. Set `base_url` without a trailing slash: normally `http://127.0.0.1:8000`. The portable Windows share package uses `http://127.0.0.1:18000` instead.
2. Set `client_id` to the **Postman public client ID** printed by `php artisan shop:setup --password-file=../.local/user-credentials.json`. Use the ID from the same database as the running backend. The desktop WPF client ID has a different registered callback and cannot be substituted.
3. Open the collection's Authorization tab and choose **OAuth 2.0**. Under Configure New Token, select **Authorization Code (With PKCE)** and enter:

| Setting | Value |
| --- | --- |
| Callback URL | `https://oauth.pstmn.io/v1/browser-callback` |
| Authorize using browser | Enabled |
| Auth URL | `{{base_url}}/oauth/authorize` |
| Access Token URL | `{{base_url}}/oauth/token` |
| Client ID | `{{client_id}}` |
| Client secret | Leave empty |
| Code challenge method | SHA-256 / S256 |
| Scope | `shop:access` |
| State | A newly generated random 32–256 character base64url string |

You can generate a suitable state in PowerShell with `[guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')`. Generate a new value for each sign-in. Let Postman generate the PKCE verifier.

4. Click **Get New Access Token**, sign in with `owner@icecream.test` and your own local password, approve access if prompted, and select **Use Token**. Keep the token private and do not sync or export populated secrets.
5. Send **Current authenticated user**, then **List categories and select first category**. The category request captures `category_id` in the selected environment.

The imported collection initially uses Bearer `{{access_token}}`. If you prefer to keep that configuration, copy your newly obtained access token into the environment's local `access_token` value instead of switching the collection to OAuth 2.0. Tokens expire after 15 minutes; repeat sign-in when needed. There is no client secret or refresh-token workflow.

## Demonstrate CRUD

Run the Inventory folder in order, then the Customer folder in order, while signed in as Owner. Each folder creates a synthetic demo record, saves its ID and version, reads it, updates it, and deletes it. Run against a local demonstration database. Keep these IDs associated with the records the collection created.

The saved version is automatically refreshed after reads and writes. PATCH sends `version` in JSON. DELETE sends the quoted version in `If-Match`, with no body or query parameters. If a record changes elsewhere, read it again before repeating a write.

The collection includes response status assertions and captures IDs and versions. These scripts have been checked against the source but have not been executed against a live server as part of preparing these files.

## Sign-in helper requests

The first two requests demonstrate CSRF-protected session login independently of OAuth: GET `/login` captures the form's CSRF token; POST `/api/auth/login` uses the same cookie jar plus the local `email` and `password` environment values. This response confirms a browser session and does not return an API access token.

The third helper exchanges a manually obtained authorization code and matching PKCE verifier at `/oauth/token`. Set `authorization_code` and `code_verifier` only when following a manual OAuth flow. Most reviewers should use the Postman OAuth token helper above instead. Do not run the Sign-in helpers folder in an automated CRUD run.

## Permission and validation checks

Run these requests individually. The first three check unauthenticated access and invalid data. For **Manager cannot delete inventory**, obtain a Manager token and set `permission_item_id` to an existing inventory record. For **Cashier cannot create inventory**, obtain a Cashier token. Confirm the role with `/api/auth/me` before each role-specific check. Do not run the Cashier check as Owner or Manager, since those roles are allowed to create the supplied demo item.

Owner can perform all CRUD operations. Manager can add/read/edit items and customers but cannot delete either. Cashier can read/edit items and add/read/edit customers, but cannot add inventory or delete either resource.

Send Logout last to revoke the current access token. Do not run the entire collection in one pass: the sign-in helpers and permission checks require different manual setups.
