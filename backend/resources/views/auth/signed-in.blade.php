@extends('auth.layout')
@section('title','Signed in')
@section('content')
<h1>You are signed in.</h1><p class="muted">Return to the desktop application and continue connecting to your shop.</p><a class="button" href="/auth/sign-out">Sign out or switch account</a>
@endsection
