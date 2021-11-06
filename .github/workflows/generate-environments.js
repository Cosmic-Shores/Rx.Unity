const buildMatrix = (matrix) => {
    let output;
    for (let category in matrix) {
        const categoryObjs = matrix[category].map(value => {
            const r = {};
            r[category] = value;
            return r;
        });
        if (output == undefined) {
            output = categoryObjs;
            continue;
        }
        
        output = output.flatMap(entry => categoryObjs.map(categoryObj => {
            return { ...entry, ...categoryObj };
        }));
    }
    return output;
}

const getRunsOn = (targetPlatform) => {
    if (targetPlatform.indexOf('Windows') !== -1)
        return 'windows-latest';
    if (targetPlatform.indexOf('OSX') !== -1)
        return 'macos-latest';
    return 'ubuntu-latest';
};
const canBeTested = (targetPlatform) => {
    return targetPlatform.indexOf('Standalone') !== -1;
};
const matrixIncludes = buildMatrix({
    targetPlatform: [
        "StandaloneLinux64",
        "StandaloneWindows",
        "StandaloneWindows64",
        "StandaloneOSX",
        "Android"
    ],
    scriptBackend: [
        "Mono2x"
    ]
}).map(obj => {
    return {
        ...obj,
        os: getRunsOn(obj.targetPlatform),
        runTests: canBeTested(obj.targetPlatform),
        buildArtifactName: `UnitTestCli_${obj.targetPlatform}_${obj.scriptBackend}`,
        buildArtifactPath: `./bin/UnitTest/${obj.targetPlatform}_${obj.scriptBackend}`
    };
});

const content = JSON.stringify({include: matrixIncludes}).replace(/\"/g, "\\\"");
const fs = require('fs');
fs.writeFileSync('./environments.json', content);
