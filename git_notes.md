


switch to main
git checkout main


git pull origin main


# Merge iteration branch back into main if it works 
DO NOT PUSH FUTURE ITERATIONS TO OLD ITERATIONS 
git merge iteration_no.1

git push main


make new iteration branch
git checkout -b iteration_no.1

add to it at leaast it contains all of main 
test.txt 
git add commit not push yet 

but does it contain all of main or all of last branch 
new branches are literally like a branch - it contains all of what is before but new stuff is not connected. Its not a new blabnnk template its
 all of your work but maybe a new feature that you dont want to connect to main as it might not work 

set the test isolated branch to be connected to test isolated remote repos 
git push --set-upstream origin iteration1
this pushes to new branch but make sure your on the actually branch git checkout 

