﻿using System.Linq;
using System.Xml.Linq;
using OneNoteObjectModel;

namespace OnenoteCapabilities
{
    public class PeopleSmartTagProcessor : ISmartTagProcessor
    {
        private SettingsPeoplePages settings;
        private Section peopleSection;

        public PeopleSmartTagProcessor(SettingsPeoplePages settings)
        {
            this.settings = settings;
            this.peopleSection = OneNoteApplication.Instance.GetNotebook(settings.PeoplePagesNotebook)
                .PopulatedSection(settings.PeoplePagesSection);
        }

        public bool ShouldProcess(SmartTag st, OneNotePageCursor cursor)
        {
            if (settings.People().Contains(personFromPersonTag(st))) return true;
            return false;
        }

        public void Process(SmartTag smartTag, XDocument pageContent, SmartTagAugmenter smartTagAugmenter, OneNotePageCursor cursor)
        {
            // TODO: Create Person Page if not Exists.
            var personPageTitle = settings.PersonNextTitle(personFromPersonTag(smartTag));

            // get PersonPage 

            var peoplePage = peopleSection.GetPage(personPageTitle);
            // IMPORTANT: The smartTag has a cache of the pageContent, so if we're writing the todo on the smartTag page via another pageContent, it will be overwritten.
            var peoplePageContent = (peoplePage.ID == cursor.PageId) ? pageContent : OneNoteApplication.Instance.GetPageContentAsXDocument(peoplePage);

            const int toPersonTableCountOnPage = 0;
            const int fromPersonTableCountOnPage = 1;

            var tableOnPage = IsFromPerson(smartTag) ? fromPersonTableCountOnPage : toPersonTableCountOnPage;
            DumbTodo.AddToPageFromDateEnableSmartTag(peoplePageContent, smartTag, tableOnPage: tableOnPage);


            smartTag.SetLinkToPageId(peoplePage.ID);
        }

        public string HelpLine()
        {
            return "<b>#person</b> add a SmartTodo for the person from the rest of this line";
        }

        public bool IsFromPerson(SmartTag smartTag)
        {
            return smartTag.TagName().StartsWith("from");
        }

        public string personFromPersonTag (SmartTag smartTag)
        {
            if (IsFromPerson(smartTag))
            {
                return smartTag.TagName().Substring(4);
            }
            else
            {
                return smartTag.TagName();
            }
        }
    }
}
