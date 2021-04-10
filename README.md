# Unity & GitHub Actions Automatic Test Example

An example project for automated testing using Unity and GitHub Actions.

> Qiita: [UnityとGitHubActionを使って自動テストを行う](https://qiita.com/makihiro_dev/private/fda3fa840f5311d2b3d5)

## 🔰 Process

### 1. Prepare Tests Repository

Prepare a repository for the Unity project that contains the tests.

### 2. Acquire ULF

Acquire the ULF file to activate your Unity license.
Use the following tool to acquire the ULF file.

https://github.com/mackysoft/Unity-ManualActivation

### 3. Register ULF to Secrets

1. Select the `Settings > Secrets` menu in the project repository.
2. Click the `New repository secret` button.
3. Enter "UNITY_LICENSE" in Name and copy and paste the contents of the ULF file in Value.
4. Click the `Add secret` button.

You can now treat the contents of the ULF file as an environment variable while keeping its contents private.

### 4. Write a YAML file to run the tests.

Create a `Test.yaml` file (you can name it anything you want) under the `.github/workflows/` folder, and write the process to run the tests there.

```yaml:Test.yaml
name: Test

on: [push, pull_request]

jobs:
  test:
    name: ${{ matrix.testMode }} on ${{ matrix.unityVersion }}
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix:
        projectPath:
          - .
        unityVersion:
          - 2020.3.1f1 # Enter the Unity version of the ULF you registered in Secrets.
        testMode:
          - playmode
          - editmode
    steps:
      # Checkout
      - uses: actions/checkout@v2
        with:
          lfs: true

      # Cache
      - uses: actions/cache@v2
        with:
          path: ${{ matrix.projectPath }}/Library
          key: Library-${{ matrix.projectPath }}
          restore-keys: |
            Library-

      # Test
      - uses: game-ci/unity-test-runner@v2
        id: tests
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        with:
          projectPath: ${{ matrix.projectPath }}
          unityVersion: ${{ matrix.unityVersion }}
          testMode: ${{ matrix.testMode }}
          artifactsPath: ${{ matrix.testMode }}-artifacts
          checkName: ${{ matrix.testMode }} Test Results

      # Upload Artifact
      - uses: actions/upload-artifact@v2
        if: always()
        with:
          name: Test results for ${{ matrix.testMode }}
          path: ${{ steps.tests.outputs.artifactsPath }}
```

### 5. Run tests on GitHub Actions

In the case of the above `Test.yaml`, the test will be executed by GitHub Actions every time a Push or Pull Request is made.

If you see a ✔ mark as shown in the image below, you have succeeded.

![TestResult](https://user-images.githubusercontent.com/13536348/114280859-3f139b00-9a76-11eb-9299-72cdfe1b45ea.jpg)


#  📔 Author Info

Hiroya Aramaki is a indie game developer in Japan.

- Blog: [https://mackysoft.net/blog](https://mackysoft.net/blog)
- Twitter: [https://twitter.com/makihiro_dev](https://twitter.com/makihiro_dev)


#  📜 License

This library is under the MIT License.