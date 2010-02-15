#pragma once

public class ShapeCustomData
{
public:
	ShapeCustomData(void);
	~ShapeCustomData(void);
	int Id;
	char* Custom;
	bool MarkedForDeletion;
	bool MarkedForUpdate;
	b2Shape* NewShape;

};
