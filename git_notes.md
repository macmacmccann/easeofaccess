

### Stop using just 'git push'

git push origin iterationNUMBER
not git push 
its so easy to make a mistake and takes a while to redo 
git push is bad practice
define the branch when you push


### Local Repos but what branch

git status 
git branch
    * means branch your on LOCALLY
git branch -a
    list all available branches 
    (git clone does actually pull all branches)
    * mean is the one your on
    if you see remote/branchnew - git fetch 
        it means you dont have the remote branches locally
            unlikely
git checkout iterationIWANT or git switch
NOW CHECK
git branch .. read branch your on



### You pushed to wrong branch -> Proved solution 

Dont hard reset if you pushed to wrong iteration 

Local respository bash open

git status 
git branch
    youll see the branches
    * means your on that one

git log --oneline  lists the commits 
commit hash is the 7 digit number its not an actual hash

git switch iteration*
git cherry-pick "'hash'number"
    this copies the COMMIT you defined from any branch onto your active branch

now 
git push origin iteration*

dont forget 

iteration*previous has the new sprint commit

git switch iteration*previous
git reset --hard HEAD~1

    not this is dangerous
        if did not push your existing code to the new branch then all new work is reverted to old work - even if you saved locally !
        - this can be redone with a git cmd 
        git lists old branch
          git redo hashnumber 
    MAKE SURE : your on the old branch 
            git checkout 
            git switch iteration*oldbranch

 in abstract it moves the old branch back by one commit ( one mistaken push )


 git push --force origin iterationOld
    forces github to matches the ranch on github with local version


You committed to the wrong branch ( didnt push )

git reset --soft HEAD~1
    removes commit 
    keeps all changes
git switch iterationYouWant
git commit -m 'im on the right branch"
git push origin iterationIwant




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

