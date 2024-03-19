# KorpiEngine internal Style Guide

## Introduction

This document is a guide for the KorpiEngine developers to follow when writing code for the engine.
It is important to follow these guidelines to ensure that the code is consistent and maintainable.

---

## General

- Prefer readability and maintainability over performance where possible.
  - As a general rule, code should be written to be as readable and maintainable as possible. Performance should only be considered when necessary.
  - As an example, prefer using a foreach loop over a for loop when iterating over a collection, as foreach demonstrates more specific intent and is easier to read.

- Write self-explanatory code.
  - Code should be written in such a way that it is easy to understand what it does without needing comments.
  - This means using descriptive names for variables, functions, and classes, and writing code that is easy to read and understand.

### Indentation

- Use 4 spaces for indentation.

### Comments

- Use comments to explain the purpose of the code, NOT what the code does. This is what self-explanatory code is for.

### Documentation

- Use XML documentation for classes, functions, and properties.
- Classes, functions, and properties should be documented with a summary, and any parameters or return values should also be documented.

### Magic Numbers

> Magic numbers are numbers that are used directly in code without explanation, and are often unclear in meaning.
- Avoid using magic numbers in code.
- Instead, use constants to give these numbers a name and make their purpose clear.

### Testing

- Write unit tests for all code where possible.
- Design code to be modular and testable where possible.

---

## Naming Conventions

### General

> Always use descriptive names for variables, functions, and classes.
- Method (function) names: PascalCase.
- Class names: PascalCase.
- Constants: UPPERCASE_WITH_UNDERSCORES.

### Class Names

- Use a singular noun or noun phrase for class names.

### Function Names

- Use a descriptive verb or verb phrase for function names.

### Variable Names

- Use a name that describes the purpose of the variable.
- Use meaningful names that are easy to understand.
- Prefer longer names over shorter ones if it makes the purpose of the variable clearer.