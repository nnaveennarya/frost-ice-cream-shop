<?php

declare(strict_types=1);

namespace App\Exceptions;

use Symfony\Component\HttpKernel\Exception\HttpException;

final class ApiProblem extends HttpException
{
    public function __construct(int $status, public readonly string $errorCode, string $message)
    {
        parent::__construct($status, $message);
    }
}
