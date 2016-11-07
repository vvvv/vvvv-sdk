param($installPath, $toolsPath, $package, $project)

# Set the copy local flag to false
foreach ($reference in $project.Object.References)
{
    if ($reference.Name -eq $package.Id)
    {
        $reference.CopyLocal = $false;
    }
}