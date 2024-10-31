### Release Notes: LowlandTech.Core v1.0.1

**Release Date:** October 23, 2024

* * *

### New Features:

-   **Compute SHA-256 Hash**:
    -   Added a new extension method `ComputeSha256Hash` to the `StringExtensions` class. This method computes the SHA-256 hash of a given string input, providing a reliable way to generate a fixed-length hash.
    -   Example usage:
        
        csharp
        
        Copy code
        
        `string input = "Hello, World!"; string hash = input.ComputeSha256Hash(); // hash = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f"`
        
    -   This method is especially useful for scenarios requiring content verification, caching mechanisms, and tracking changes to files by comparing hashes.

### Improvements:

-   **Enhanced `ToPlural` Method**:
    -   Updated the `ToPlural` method in `StringExtensions` to handle more edge cases and ensure consistency in pluralization.
    -   Improvements include:
        -   Special cases like converting "mouse" to "mice" and "man" to "men" are handled using a predefined dictionary of irregular plural forms.
        -   Added support for words ending in "y" (e.g., "city" to "cities" but "key" to "keys").
        -   Enhanced logic for words ending with "us," "f/fe," and others.
        -   Improved handling for empty strings and null inputs to return an empty string without adding an "s".
    -   Example usage:
        
        csharp
        
        Copy code
        
        `string singular = "city"; string plural = singular.ToPlural(); // plural = "cities"  singular = "child"; plural = singular.ToPlural(); // plural = "children"  singular = "book"; plural = singular.ToPlural(); // plural = "books"`
        

### Bug Fixes:

-   **Fixed Hash Mismatch in Tests**:
    -   Corrected test cases for `ComputeSha256Hash` that previously expected incorrect hash values.
    -   Ensured consistent handling of whitespace and character encoding during hash computation to avoid mismatches between expected and actual values.

### Summary:

This release focuses on adding the `ComputeSha256Hash` functionality, enhancing the `ToPlural` method, and improving the overall robustness of string manipulation utilities in `LowlandTech.Core`. These enhancements are intended to support better data validation, file tracking, and string transformation use cases.

* * *

### Upgrade Guidance:

-   Update your package reference to v1.0.1 in your projects to take advantage of the new `ComputeSha256Hash` and improved `ToPlural` methods.
-   Ensure that your project is using the latest .NET SDK version as specified in the release.

**Thank you for using LowlandTech.Core!**

