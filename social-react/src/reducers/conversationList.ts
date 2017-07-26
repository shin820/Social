import { ConversationListAction, SEARCH_CONVERSATION_LIST, MARK_AS_READ } from '../actions/conversationList';
import { ConversationList } from '../types/conversation';

export default function conversationList(state: ConversationList, action: ConversationListAction): ConversationList {
    switch (action.type) {
        case SEARCH_CONVERSATION_LIST:
            return state;
        case MARK_AS_READ:
            return state;
    }

    return state;
}