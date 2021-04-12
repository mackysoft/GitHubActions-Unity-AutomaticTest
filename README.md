# Unity & GitHub Actions Automatic Test Example

An example project for automated testing using Unity and GitHub Actions.

> Qiita: [UnityとGitHubActionを使って自動テストを行う](https://qiita.com/makihiro_dev/private/fda3fa840f5311d2b3d5)

## 🔰 Tutorial

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

## Extra: Report results to Slack

A tutorial on how to report the results of a test to Slack.

### 1. Generate and acquire Webhook URL

In order to send messages from GitHub Actions to Slack, you need an Incoming Webhook URL of Slack.

Follow the instructions on the following page to generate a webhook URL.

https://api.slack.com/messaging/webhooks

If you follow the steps, a URL starting with `https://hooks.slack.com/services/` will be generated in the Webhook URL, and you can use that URL after this.

![WebhookURL](https://user-images.githubusercontent.com/13536348/114370729-bfdbaf80-9bba-11eb-8fbd-9deb2bfd7b7f.jpg)


### 2. Register Webhook URL to Secrets

Register the acquired Webhook URL to `Settings > Secrets` in the project repository.

Add a new secret, enter `SLACK_HOOK` in Name and enter Webhook URL in Value.

![SLACK_HOOK](https://user-images.githubusercontent.com/13536348/114371707-bb63c680-9bbb-11eb-8fdd-d198ad7946f4.jpg)


### 3. Write the Slack reporting process in YAML for testing

Add the Slack reporting process to the YAML where you wrote the test process.

```yaml:test.yaml
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
          - 2020.3.1f1
        testMode:
          - playmode
          - editmode
    steps:
      # Checkout
      - name: Checkout
        uses: actions/checkout@v2
        with:
          lfs: true

      # Cache
      - name: Cache
        uses: actions/cache@v2
        with:
          path: ${{ matrix.projectPath }}/Library
          key: Library-${{ matrix.projectPath }}
          restore-keys: |
            Library-

      # Tests
      - name: Tests
        uses: game-ci/unity-test-runner@v2
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
      - name: Upload Artifact
        uses: actions/upload-artifact@v2
        if: always()
        with:
          name: Test results for ${{ matrix.testMode }}
          path: ${{ steps.tests.outputs.artifactsPath }}
          
  reportSlack:
    name: ${{ matrix.testMode }} report
    runs-on: ubuntu-latest
    strategy:
      matrix:
        testMode:
          - playmode
          - editmode
    needs: test
    steps:
    
      # Download Artifact
      - name: Download Artifact
        uses: actions/download-artifact@main
        with:
          name: Test results for ${{ matrix.testMode }}
          path: ${{ matrix.testMode }}-artifacts

      # Clone NUnitXmlReporter
      - name: Clone NUnitXmlReporter
        run: git clone https://github.com/pCYSl5EDgo/NUnitXmlReporter.git

      # Setup .NET
      - name: Setup .NET
        uses: actions/setup-dotnet@v1.7.2
        with:
          dotnet-version: '3.0.100'
      
      # Report Result to Slack
      - name: Report Result to Slack
        env:
          SLACK: ${{ secrets.SLACK_HOOK }}
        run: |
          cd NUnitXmlReporter
          dotnet run ../${{ matrix.testMode }}-artifacts/${{ matrix.testMode }}-results.xml ../slackJson --slack-block  $GITHUB_REPOSITORY $GITHUB_SHA || INPUT_RESULT=$?
          cd ..
          curl -X POST -H 'ContentX-type:application/json' --data "$(cat slackJson)" $SLACK
          exit $INPUT_RESULT
```

## 4. Run tests

1. Push or Pull Request to the repository and run the testing workflow.
2. Wait for the workflow to complete.

If the test results are reported to Slack as shown below, you have succeeded.

![SlackTestReport](https://user-images.githubusercontent.com/13536348/114363192-1e9d2b00-9bb3-11eb-90d1-b6f83a069f59.jpg)

#  📔 Author Info

Hiroya Aramaki is a indie game developer in Japan.

- Blog: [https://mackysoft.net/blog](https://mackysoft.net/blog)
- Twitter: [https://twitter.com/makihiro_dev](https://twitter.com/makihiro_dev)


#  📜 License

This repository is under the MIT License.
