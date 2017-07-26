export class ConversationListSearch {
    keywords: string;
    since: Date;
    until: Date;
    currentPage: number;
    pageSize: number = 50;
}

export class ConversationList {
    items: ConversationListItem[] = [];
    search: ConversationListSearch;
    total: Number;
}

export class ConversationListItem {
    id: number;
    source: number;
    ifRead: boolean;
    status: number;
    subject: string;
    priority: number;

}