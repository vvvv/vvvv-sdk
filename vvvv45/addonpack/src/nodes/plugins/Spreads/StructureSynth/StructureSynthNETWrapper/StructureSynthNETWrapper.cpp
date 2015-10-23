#include "stdafx.h"
#include "StructureSynthNETWrapper.h"

#include "Parser/Tokenizer.h"
#include "Parser/Preprocessor.h"
#include "Parser/EisenParser.h"
#include "Model/Builder.h"
#include "Model/Ruleset.h"
#include "Model/Rendering/TemplateRenderer.h"

#include <QString>
#include <QClipBoard>
#include <QApplication>

using namespace StructureSynth::Parser;
using namespace StructureSynth::Model;
using namespace StructureSynth::Model::Rendering;
using namespace System::Runtime::InteropServices;


namespace StructureSynthNETWrapper 
{
	System::Int32^ TestTokenizer::Count(System::String^ str) 
	{
		char* str2 = (char*)(void*)Marshal::StringToHGlobalAnsi(str);

		//CString cs(str);
		QString stri(str2);

		

		TemplateRenderer rendering("D:\\povray.rendertemplate");
		//Preprocessor::Process(str2);
		//POVRenderer rend(render*);
		//QChar(
			//CString str(
		rendering.begin();
		Tokenizer tok(Preprocessor::Process(str2));
		//tok.symbols.count();
		EisenParser e(&tok);
		RuleSet* rs = e.parseRuleset();
		rs->resolveNames();
		rs->dumpInfo();
		Builder b(&rendering, rs);
		b.build();
		rendering.end();
		//tok.getSymbol();

		//QClipboard *clipboard = QApplication::clipboard();
		//clipboard->setText(rendering.getOutput()); 
		std::string res = rendering.getOutput().toStdString();

		unsigned int ui = res.length();
		
		System::Int32^ test = Convert::ToInt32(ui);  //
		return test;
	}


}

