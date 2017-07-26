export const SEARCH_CONVERSATION_LIST = 'SEARCH_CONVERSATION_LIST';
export type SEARCH_CONVERSATION_LIST = typeof SEARCH_CONVERSATION_LIST;
export interface Search {
    type: SEARCH_CONVERSATION_LIST
}

export const MARK_AS_READ = 'MARK_AS_READ';
export type MARK_AS_READ = typeof MARK_AS_READ;
export interface MarkAsRead {
    type: MARK_AS_READ
}

export type ConversationListAction = Search | MarkAsRead;

export function Search() {
    return {
        type: SEARCH_CONVERSATION_LIST
    }
}

export function MarkAsRead() {
    return {
        type: MARK_AS_READ
    }
}