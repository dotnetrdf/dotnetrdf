# Deprecation Policy

Our deprecation policy is a very simple three step process.

## Step 1 - Obsolete but Usable

Firstly APIs to be removed **must** be marked with the `Obsolete` attribute with the boolean parameter set to false such that the code can still be used. The warning message **should** direct users to alternative/replacement APIs that are available and when we expect to remove the API.

There **must** be at least one release where this is present and the change **must** be noted in the Change Log for that release.

If the API is included in any documentation on the website it **should** be updated to indicate that the API is being deprecated or to use the alternative APIs

## Step 2 - Obsolete

In a release subsequent to the `Obsolete` attribute having been added the boolean parameter **must** be set to true so that using this code now results in a compiler error. The warning message **may** be updated to further clarify alternative/replacement APIs and when we will remove the API.

Again there **must** be at least one release where this is present and the change **must** be noted in the Change Log for that release. Ideally an API should only remain in this state for a single release.

If the API is included in any documentation on the website it **must** be updated to indicate that the API is obsolete and documentation is preserved only for historical reference.

### Step 3 - Remove

The API **must** be fully removed from the library and this **must** be noted in the Change Log of the release where this happens.

Documentation pertaining to this API **must** be removed or clearly marked as pertaining to an obsolete unsupported API.