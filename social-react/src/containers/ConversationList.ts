import ConversationList from '../components/ConversationList/ConversationList';
import * as actions from '../actions/conversationList';
import { ApplicationState } from '../types/index';
import { connect, Dispatch } from 'react-redux';

export function mapStateToProps({ ConversationList }: ApplicationState) {
    return {
        list: ConversationList.items,
        keywords: ConversationList.search.keywords
    }
}

export function mapDispatchToProps(dispatch: Dispatch<actions.ConversationListAction>) {
    return {
        onSearch: () => dispatch(actions.Search()),
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(ConversationList);