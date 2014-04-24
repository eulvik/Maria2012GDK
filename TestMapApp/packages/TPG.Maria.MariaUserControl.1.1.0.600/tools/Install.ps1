param($installPath, $toolsPath, $package, $project)

    $packageName = $package.Id + '.' + $package.Version;
    $packageId = $package.Id;
 
    # Full assembly name is required
    Add-Type -AssemblyName 'Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
 
    $projectCollection = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection
     
    # There is no indexer on ICollection<T> and we cannot call
    # Enumerable.First<T> because Powershell does not support it easily and
    # we do not want to end up MethodInfo.MakeGenericMethod.
    $allProjects = $projectCollection.GetLoadedProjects($project.Object.Project.FullName).GetEnumerator(); 
	
    if($allProjects.MoveNext())
    {
        foreach($item in $allProjects.Current.GetItems('Reference'))
        {
            $hintPath = $item.GetMetadataValue("HintPath")
            $newHintPath = $hintPath -replace "$packageId.*?\\", "$packageName\"
            if ($hintPath -ne $newHintPath)
            {
                Write-Host "Updating $hintPath to $newHintPath"
                $item.SetMetadataValue("HintPath", $newHintPath);
            }
        }
    }