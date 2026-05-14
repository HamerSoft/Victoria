#mkdir Root

#mv ./Editor Root/Editor
#mv ./Runtime Root/Runtime
#mv ./Tests Root/Tests
#mv package.json Root/package.json

mkdir Assets
mkdir ProjectSettings
mkdir -p Packages/com.hamersoft.betterresources
mv ./Editor Packages/com.hamersoft.betterresources
mv ./Runtime Packages/com.hamersoft.betterresources
mv ./Tests Packages/com.hamersoft.betterresources
mv package.json Packages/com.hamersoft.betterresources/package.json