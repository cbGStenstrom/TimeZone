# Personal C# / Blazor Coding Style Guide

# TimeZone Coding Standards

These standards are mandatory for all new or modified production code.

When applying these standards:

1. Explicit requirements in the approved issue/plan take precedence when they
   intentionally override a coding standard.
2. Otherwise, this document defines the required project coding style.
3. Match surrounding code where this document does not specify a convention.
4. Do not refactor unrelated existing code merely to bring it into compliance.
5. Code touched as part of the requested change should follow these standards
   unless doing so would require unrelated restructuring.

## 1. Blazor component structure
- Use **code-behind only**. Put logic in `Component.razor.cs` as `public partial class Component : BaseClass`. **Never use `@code` blocks.**
- The `.razor` file contains markup, `@using`, `@inherits`, and `@attribute` directives only.
  - Order the directives: `@using` lines, a blank line, `@inherits`, then `@attribute [Route(PageUrls.XxxUrl)]`.
  - Route URLs come from a `PageUrls` constants class, never string literals.
- Bind to members with an explicit `@this.` prefix in markup (e.g. `OnTileClick="@this.Handler"`). Use `@` before every bound value.
- In markup:
  - Indent 4 spaces.
  - Put each component parameter on its own line, aligned under the first attribute.
  - Close components with a full end tag (`</Comp>`), not self-closing.
  - Use blank lines to separate logical sections.
- Use Telerik components (e.g. `TelerikStackLayout`, `TelerikGrid`). Don't replace them with custom HTML.
- Generate control IDs with `StringUtilities.CreateControlId()`.

## 2. File layout
- Use file-scoped namespaces (`namespace X.Y;`).
- Put `using` directives at the top, with `System.*` first and the rest alphabetical.
- Declare one partial class per file.

## 3. Regions
- Group members into `#region` blocks by purpose. Always close with a matching name: `#endregion <same name>`.
- Use lowercase region names.
- Put a blank line after `#region` and before `#endregion`.
- Use these regions, in this order, and omit any that are empty:
  1. `constants`: private constants and nested static classes. For example, a `LogMessages` static class holding `const string` messages.
  2. `dependencies`: `[Inject]` properties.
  3. `data`: data loading and saving methods.
  4. `properties`: state and reference properties.
  5. `lifecycle`: `OnInitializedAsync`, `OnParametersSet`, `Dispose`, and so on.
  6. `event handlers`: UI and component event callbacks.
  7. `private`: private helper methods.
- Within each region, sort members **alphabetically by member name**.
  - I'm not sure the existing file follows this strictly in the `data` and `private` regions, so treat alphabetical order as the rule.
- Add a `parameters` region (`[Parameter]` properties) before `dependencies` when the component has parameters.

## 4. Members
- Use **properties instead of fields**. Use auto-properties with initializers, such as `{ get; set; } = [];`.
- Use `= default!;` for non-nullable references that are set later, such as `@ref` components and injected services. Use `?` for values that may legitimately be null.
- Avoid the `!` null-forgiving operator.
- Use `[Inject]` on private properties, not constructor injection, in components.
- Use `this.` for instance members. Use `base.` for inherited members (e.g. `base.AppDataSvc`, `base.CancellationToken`).
- Use `var` when the type is obvious from the right-hand side. Use explicit types for simple values like `int userId = ...`.
- Use collection expressions (`[]`) and target-typed `new(...)`.
- Use async methods with a `CancellationToken cancellationToken = default` parameter, and pass `base.CancellationToken` from components.
- Use braces for multi-line bodies. Single-line guard clauses are fine: `if (x == null) return;`.
- Always call `await base.OnInitializedAsync();` first in lifecycle overrides.
- Name event handlers:
  - `ComponentName_OnEvent` for handlers wired to a child component's event, such as `QuickLinksComponent_OnTileClick`.
  - `OnComponentNameEvent` is also acceptable.

## 5. Comments and documentation
- Add **XML doc comments to every member**, including private ones.
  - Use `<summary>` on its own, then `<remarks>` for behavior details such as side effects or edge cases.
  - Use `<param>` and `<returns>` where relevant.
  - Indent summary text with an extra space (`///  Text`) and wrap lines at about 100 characters.
- Use `// WHAT:` and `// WHY:` inline comments above non-trivial blocks.
  - `WHAT` states what the block does.
  - `WHY` states why it is needed. Include it when the reason isn't obvious.
  - Wrapped continuation lines are indented two spaces after `//`.
- Don't add comments that only restate the code.

## 6. Constants and logging
- Don't use magic strings. Put log messages and parameter names in constants. For example, `PoamFilterParameterNames.UserId` and a nested `LogMessages` class.
- Use `ILogger<T>` through `[Inject]`.
- Log a warning when a required reference is unavailable.

## 7. General
- Prioritize readability over cleverness.
- Keep methods small and single-purpose.
- Use `await` and avoid `.Result` or `.Wait()`.
- Don't restructure or reformat code I didn't ask you to change.
- Use file scoped namespaces 
- Use squiggly braces for all using statements, even if they are single-line.
- Do NOT use top-level statements in any code file. All code must be contained within a class or method.
- When designing a new component, consider whether it should be a Blazor page or a reusable component. If it is a page, it should be routable and have a URL. If it is a reusable component, it should not have a URL and should be used within other components or pages.
- When designing a new page or modifying an existing one, consider how the contents of the page may be reused in other pages or components. If the contents are likely to be reused, consider creating a reusable component for that content and using it within the page. This promotes code reuse and maintainability.
- Code using DRY principles. Avoid duplicating code across components or pages. If you find yourself writing similar code in multiple places, consider refactoring it into a reusable component or service.
- Code using SOLID principles. Ensure that your code adheres to the principles of single responsibility, open/closed, Liskov substitution, interface segregation, and dependency inversion. This will help create a more maintainable and extensible codebase.
- Code using KISS principles. Keep your code simple and avoid unnecessary complexity. Simple code is easier to read, understand, and maintain.
- Structure code with separation of concerns in mind. Keep different aspects of your code (e.g., data access, business logic, UI) separate and organized. This will make it easier to maintain and test your code.
- Write code so that it's easy to read and understand. 
  - Use meaningful names for variables, methods, and classes. Avoid abbreviations and acronyms that may not be clear to other developers.
  - Use indentation and whitespace to improve readability. Consistent formatting makes it easier to follow the structure of the code.
- For the most part, follow the official Microsoft C# coding conventions, but prioritize the project-specific standards outlined in this document when they conflict with the official conventions.


## 8. Blazor Templates
When creating a routable Blazor page, use
`docs/Templates/BlazorPage/` as the structural template.

When creating a reusable Blazor component, use
`docs/Templates/BlazorComponent/` as the structural template.

These repository templates mirror the corresponding Visual Studio custom item
templates and are authoritative for code generated by agents.

## 9. UI component usage
Use Radzen components for application UI controls whenever a suitable Radzen
component exists.

Examples include:

- `RadzenButton` instead of native `<button>`
- `RadzenTextBox` instead of native `<input type="text">`
- `RadzenDropDown` instead of native `<select>`
- `RadzenCheckBox` instead of native checkbox inputs
- `RadzenDataGrid` instead of hand-built HTML tables for application data grids
- Radzen layout components where they are already the project convention

Do not mix Radzen and native HTML controls arbitrarily within the same feature.

Native HTML elements are acceptable only when:

1. no appropriate Radzen component exists,
2. the element is purely structural or semantic markup,
3. an existing project pattern explicitly uses native HTML for that purpose, or
4. the approved issue/plan specifically requires native markup.

When using a native HTML control because no Radzen equivalent is appropriate,
preserve the surrounding project style and document the reason when it is not
obvious.

Do not replace an existing Radzen component with native HTML unless the requested
change explicitly requires it.