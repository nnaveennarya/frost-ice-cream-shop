@extends('auth.layout')
@section('title','Browser sign out')
@section('content')
<h1>Finish signing out.</h1><p class="muted">End the browser sign-in session to switch accounts or complete sign-out.</p><p class="notice">The desktop application revokes its access token separately. This step clears your browser session.</p><form method="post" action="/auth/browser-logout">@csrf<button>Sign out of browser</button></form>
@endsection
