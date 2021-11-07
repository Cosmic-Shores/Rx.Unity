const { src, dest, parallel } = require("gulp");
const replace = require("gulp-replace");
const filter = require("gulp-filter");
const print = require('gulp-print').default;
const path = require('path');

class Tasks {
    patchRxMainCsProj() {
        return src("../reactive/Rx.NET/Source/src/System.Reactive/System.Reactive.csproj")
        .pipe(replace("<TargetFrameworks>netcoreapp3.1;netstandard2.0;net472;uap10.0.16299;net5.0;net5.0-windows10.0.19041</TargetFrameworks>", "<TargetFramework>netstandard2.0</TargetFramework><LangVersion>9</LangVersion><DefineConstants>NO_NULLABLE_ATTRIBUTES</DefineConstants>"))
        .pipe(replace("<SignAssembly>true</SignAssembly>", "<SignAssembly>false</SignAssembly>"))
        .pipe(dest((file) => file.base));
    }
    
    patchRxRemoveUnsupportedFeatures() {
        return src("../reactive/Rx.NET/Source/src/System.Reactive/TaskObservable.cs")
        .pipe(replace("[AsyncMethodBuilder(typeof(TaskObservableMethodBuilder<>))]", ""))
        .pipe(replace("<SignAssembly>true</SignAssembly>", "<SignAssembly>false</SignAssembly>"))
        .pipe(dest((file) => file.base));
    }

    testCode() {
        const threadPoolSchedulerReplacementFilter = filter([
            "**",
            "!Tests/Concurrency/SchedulerTest.cs",
            "!Tests/Concurrency/ThreadPoolSchedulerTest.cs",
        ], {restore: true});
        return this.xunit2nunitTransformer(src("./**/*.cs", {
            cwd: path.join(__dirname, "../reactive/Rx.NET/Source/tests/Tests.System.Reactive"),
            ignore: [
                "Tests/Internal/HalfSerializerTest.cs",
                "Tests/LicenseHeaderTest.cs",
                "Tests/ArgumentValidationTest.cs",
                "Tests/Aliases.cs",
                "Tests/AliasesTest.cs",
                "Tests/TaskLikeSupportTest.cs",
                "Tests/Linq/ObservableRemotingTest.cs",
                "Tests/Linq/ObservableSafetyTest.cs",
                "Tests/Linq/QbservableTest.cs",
                "Tests/Linq/QbservableExTest.cs",
                "obj/**/*.cs"
            ],
            nocase: true
        })
        .pipe(print(filepath => `built: ${filepath}`)))
        .pipe(threadPoolSchedulerReplacementFilter)
        .pipe(replace("ThreadPoolScheduler.Instance", "Rx.Unity.Concurrency.ThreadPoolOnlyScheduler.Instance"))
        .pipe(threadPoolSchedulerReplacementFilter.restore)
        .pipe(replace("public class ", "public partial class "))
        .pipe(replace("public void ForEachAsync_DisposeThrows()", "private void ForEachAsync_DisposeThrows()"))
        .pipe(replace("public void Virtual_ThreadSafety()", "private void Virtual_ThreadSafety()"))        
        .pipe(replace("public void Generate_TimeSpan_DisposeLater()", "private void Generate_TimeSpan_DisposeLater()"))
        .pipe(replace("public void Generate_DateTimeOffset_DisposeLater()", "private void Generate_DateTimeOffset_DisposeLater()"))
        .pipe(dest(path.join(__dirname, "../Tests/Tests.System.Reactive")));
    }

    testingLibCode() {
        return this.xunit2nunitTransformer(src("**/*.cs", {
            cwd: path.join(__dirname, "../reactive/Rx.NET/Source/src/Microsoft.Reactive.Testing"),
            ignore: [
                "obj/**/*.cs",
                "Properties/AssemblyInfo.cs"
            ],
            nocase: true
        }))
        .pipe(print(filepath => `built: ${filepath}`))
        .pipe(dest(path.join(__dirname, "../Tests/Microsoft.Reactive.Testing")));
    }

    rxDataTestCode() {
        return src("**/*.cs", {
            cwd: path.join(__dirname, "../Rx.Data/src/Rx.Data.Tests"),
            ignores: "obj/**/*.cs",
            nocase: true
        })
        .pipe(print(filepath => `built: ${filepath}`))
        .pipe(dest(path.join(__dirname, "../Tests/Rx.Data.Tests")));
    }

    xunit2nunitTransformer(source) {
        return source
            .pipe(replace("using Xunit;", "using NUnit.Framework;\r\nusing Rx.Unity.Tests.Helper;"))
            .pipe(replace("Assert.Equal(", "XunitAssert.Equal("))
            .pipe(replace("Assert.NotEqual(", "XunitAssert.NotEqual("))
            .pipe(replace("Assert.IsType<", "XunitAssert.IsType<"))
            .pipe(replace("Assert.IsType(", "XunitAssert.IsType("))
            .pipe(replace("Assert.Same(", "Assert.AreSame("))
            .pipe(replace("Assert.NotSame(", "Assert.AreNotSame("))
            .pipe(replace("Assert.Empty(", "CollectionAssert.IsEmpty("))
            .pipe(replace(/\[Fact([^\]]+)?\]/g, "[Test]"))
            .pipe(replace(/\[Trait\([^\(]*\)\]/g, "// $&"))
            .pipe(replace(/\[Test[^.\w]([^\(]+(?<=\sasync\s)[^\(]+\s{1,}(\w+)\s{0,}\()/gm, "[Test]\r\n        public void $2_Sync() => $2().Wait();\r\n$1"));
    }
}

const tasks = new Tasks();
exports.generateRxProj = parallel(() => tasks.patchRxMainCsProj(), () => tasks.patchRxRemoveUnsupportedFeatures());
exports.generateTests = parallel(() => tasks.testCode(), () => tasks.testingLibCode(), () => tasks.rxDataTestCode());
